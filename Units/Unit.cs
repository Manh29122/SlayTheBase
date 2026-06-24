using System;
using UnityEngine;
using MoreMountains.Feedbacks;
using SlayTheTower.Data;

namespace SlayTheTower
{
    /// <summary>
    /// Đơn vị quân di động (cận chiến hoặc tầm xa).
    /// Di chuyển qua các waypoint về phía base địch; gặp mục tiêu trong tầm thì đứng đánh.
    /// Hoạt ảnh = ĐỔI SPRITE THEO FRAME (SpriteFrameAnimator), không dùng Animator.
    /// Đòn đánh gây sát thương / bắn đạn TẠI frame chỉ định, rồi chờ cooldown giữa các đòn.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class Unit : MonoBehaviour
    {
        [Header("Hiển thị")]
        [Tooltip("Lật sprite theo hướng di chuyển/đánh (sidescroller). Sprite gốc mặc định quay PHẢI.")]
        [SerializeField] private bool flipToFaceDirection = true;

        [Header("Trúng đòn")]
        [Tooltip("MMF_Player phát khi unit trúng đòn — gắn MMF_Flicker (và juice khác) vào đây.")]
        [SerializeField] private MMF_Player damageFeedbacks;
        [Tooltip("Màu chữ floating text sát thương.")]
        [SerializeField] private Color floatingTextColor = new Color(1f, 0.85f, 0.3f);
        [Tooltip("Độ cao spawn floating text so với unit.")]
        [SerializeField] private float floatingTextYOffset = 0.7f;

        [Tooltip("Lệch so với tuyến để nhiều quân không chồng khít lên nhau.")]
        public Vector3 laneOffset = Vector3.zero;

        private UnitDefinition _def;
        private Team _team;
        private PathSystem _path;
        private Base _targetBase;
        private Health _health;
        private SpriteRenderer _sprite;
        private SpriteFrameAnimator _spriteAnim;

        private int _targetWaypointIndex;
        private int _endIndex;
        private int _step;
        private float _cooldownTimer;
        private float _retargetTimer;
        private float _stunTimer;
        private Health _target;
        private bool _isMoving;
        private bool _attacking;

        private const float WaypointReachDistance = 0.05f;
        private const float RetargetInterval = 0.2f;

        public Team Team => _team;
        public Health Health => _health;
        public UnitDefinition Definition => _def;
        public bool IsDead => _health == null || _health.IsDead;

        /// <summary>Bắn ra mỗi khi đơn vị bắt đầu 1 đòn (cho VFX/SFX nếu cần).</summary>
        public event Action OnAttack;
        public event Action<bool> OnMovingChanged;

        public void Initialize(UnitDefinition def, Team team, PathSystem path, Base targetBase)
        {
            _def = def;
            _team = team;
            _path = path;
            _targetBase = targetBase;

            _health = GetComponent<Health>();
            _health.Setup(def.maxHP);
            _health.OnDied += HandleDied;
            _health.OnDamaged += HandleDamaged;

            _sprite = GetComponentInChildren<SpriteRenderer>();
            _spriteAnim = GetComponentInChildren<SpriteFrameAnimator>();
            if (_spriteAnim == null && _sprite != null)
                _spriteAnim = _sprite.gameObject.AddComponent<SpriteFrameAnimator>();

            _step = path.Step(team);
            _endIndex = path.EndIndex(team);
            transform.position = path.GetWaypoint(path.StartIndex(team)) + laneOffset;
            _targetWaypointIndex = path.StartIndex(team) + _step;
            _cooldownTimer = 0f;
        }

        private void OnEnable() => UnitRegistry.Register(this);
        private void OnDisable() => UnitRegistry.Unregister(this);

        private void Update()
        {
            if (_def == null || IsDead) return;

            if (_stunTimer > 0f)
            {
                _stunTimer -= Time.deltaTime; // choáng: đứng yên, huỷ đòn đang đánh
                if (_attacking) { _attacking = false; _spriteAnim?.Stop(); }
                return;
            }

            if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;
            if (_attacking) return; // đang trong anim đánh -> chờ callback xử lý

            _retargetTimer -= Time.deltaTime;
            if (_retargetTimer <= 0f || _target == null || _target.IsDead || !InRange(_target))
            {
                AcquireTarget();
                _retargetTimer = RetargetInterval;
            }

            if (_target != null && !_target.IsDead && InRange(_target))
            {
                SetMoving(false);
                FaceX(_target.transform.position.x - transform.position.x);
                if (_cooldownTimer <= 0f) StartAttack();
            }
            else
            {
                MoveAlongPath();
            }
        }

        // ----- Tìm mục tiêu -----
        private void AcquireTarget()
        {
            var enemy = UnitRegistry.FindNearestEnemy(_team, transform.position, _def.attackRange);
            if (enemy != null) { _target = enemy.Health; return; }

            if (_targetBase != null && !_targetBase.Health.IsDead && InRange(_targetBase.Health))
                _target = _targetBase.Health;
            else
                _target = null;
        }

        private bool InRange(Health h)
            => h != null && Vector2.Distance(transform.position, h.transform.position) <= _def.attackRange;

        // ----- Tấn công (theo frame) -----
        private void StartAttack()
        {
            _attacking = true;
            OnAttack?.Invoke();

            if (_spriteAnim != null && _def.attackFrames != null && _def.attackFrames.Length > 0)
                _spriteAnim.PlayOnce(_def.attackFrames, _def.attackDuration, _def.attackHitFrame, OnAttackHit, OnAttackEnd);
            else
            {
                // Không có frame -> đánh tức thì rồi vào cooldown.
                OnAttackHit();
                OnAttackEnd();
            }
        }

        /// <summary>Gọi đúng frame gây sát thương: cận chiến đập, tầm xa bắn đạn.</summary>
        private void OnAttackHit()
        {
            if (_target == null || _target.IsDead) return;

            if (_def.attackType == AttackType.Ranged && _def.projectilePrefab != null)
                FireProjectile(_target);
            else if (InRange(_target))
                _target.TakeDamage(_def.attackDamage);
        }

        private void OnAttackEnd()
        {
            _attacking = false;
            _cooldownTimer = Mathf.Max(0f, _def.attackCooldown);
        }

        private void FireProjectile(Health target)
        {
            var go = Instantiate(_def.projectilePrefab, transform.position, Quaternion.identity);
            go.SetActive(true);
            var proj = go.GetComponent<Projectile>();
            if (proj == null) proj = go.AddComponent<Projectile>();
            proj.Launch(target, _def.attackDamage, _def.projectileSpeed);
        }

        // ----- Di chuyển -----
        private void MoveAlongPath()
        {
            SetMoving(true);
            Vector3 target = _path.GetWaypoint(_targetWaypointIndex) + laneOffset;
            FaceX(target.x - transform.position.x);
            transform.position = Vector3.MoveTowards(transform.position, target, _def.moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, target) < WaypointReachDistance &&
                _targetWaypointIndex != _endIndex)
            {
                _targetWaypointIndex += _step;
            }
        }

        private void SetMoving(bool moving)
        {
            if (_isMoving == moving) return;
            _isMoving = moving;
            OnMovingChanged?.Invoke(moving);
            if (moving) PlayWalk();
            else _spriteAnim?.Stop();
        }

        private void PlayWalk()
        {
            if (_spriteAnim != null && _def.walkFrames != null && _def.walkFrames.Length > 0)
                _spriteAnim.PlayLoop(_def.walkFrames, _def.walkDuration);
        }

        // ----- Lật mặt (sidescroller) -----
        private void FaceX(float dirX)
        {
            if (!flipToFaceDirection || _sprite == null || Mathf.Abs(dirX) < 0.001f) return;
            _sprite.flipX = dirX < 0f;
        }

        // ----- Trúng đòn -----
        private void HandleDamaged(float amount)
        {
            if (damageFeedbacks != null) damageFeedbacks.PlayFeedbacks();
            FloatingText.Spawn(transform.position + Vector3.up * floatingTextYOffset,
                Mathf.RoundToInt(amount).ToString(), floatingTextColor);
        }

        /// <summary>Gây choáng: đứng yên, không đánh trong X giây (cộng dồn = lấy max).</summary>
        public void ApplyStun(float duration)
        {
            if (duration > _stunTimer) _stunTimer = duration;
        }

        private void HandleDied()
        {
            // Phase 7 sẽ trả về object pool thay vì Destroy.
            Destroy(gameObject);
        }
    }
}
