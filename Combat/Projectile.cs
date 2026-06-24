using UnityEngine;

namespace SlayTheTower
{
    /// <summary>
    /// Đạn / phép bay tới mục tiêu rồi gây sát thương. Dùng cho đơn vị tầm xa (cung/wizard).
    /// Bám theo mục tiêu khi còn sống; nếu mục tiêu chết giữa đường thì bay nốt tới vị trí cũ rồi tự hủy.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        private Health _target;
        private Vector3 _lastTargetPos;
        private float _damage;
        private float _speed = 8f;
        private float _life = 5f;
        private bool _launched;

        public void Launch(Health target, float damage, float speed)
        {
            _target = target;
            _damage = damage;
            _speed = Mathf.Max(0.1f, speed);
            _lastTargetPos = target != null ? target.transform.position : transform.position;
            _launched = true;
        }

        private void Update()
        {
            if (!_launched) return;

            _life -= Time.deltaTime;
            if (_life <= 0f) { Destroy(gameObject); return; }

            if (_target != null && !_target.IsDead)
                _lastTargetPos = _target.transform.position;

            Vector3 to = _lastTargetPos - transform.position;
            float dist = to.magnitude;
            float step = _speed * Time.deltaTime;

            if (dist <= step + 0.05f)
            {
                if (_target != null && !_target.IsDead)
                    _target.TakeDamage(_damage);
                Destroy(gameObject);
                return;
            }

            transform.position += to / dist * step;
            float ang = Mathf.Atan2(to.y, to.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, ang);
        }
    }
}
