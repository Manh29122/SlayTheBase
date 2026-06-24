using System.Collections.Generic;
using UnityEngine;
using SlayTheTower.Data;

namespace SlayTheTower
{
    /// <summary>
    /// BOOTSTRAP DEMO combat 2 phe: cả hai base spawn quân (cận chiến + tầm xa) đi dọc tuyến,
    /// gặp nhau thì đánh (cận chiến giật scale thay animation; tầm xa bắn đạn/phép),
    /// tới base địch thì đập base. Bấm Play để xem.
    /// </summary>
    public class Phase2Demo : MonoBehaviour
    {
        [SerializeField] private float playerSpawnInterval = 2.0f;
        [SerializeField] private float enemySpawnInterval = 2.4f;
        [SerializeField] private float baseHP = 800f;

        private PathSystem _path;
        private Base _playerBase;
        private Base _enemyBase;

        private UnitDefinition _pMelee, _pRanged, _eMelee, _eRanged;
        private GameObject _arrowPrefab, _boltPrefab;

        private float _pTimer, _eTimer;
        private bool _spawnRangedNext;
        private static Sprite _square;
        private readonly List<Unit> _areaBuf = new();

        private void Awake()
        {
            BuildSquare();
            SetupCamera();
            BuildPath();
            BuildBases();
            BuildProjectiles();
            BuildUnitDefs();
        }

        private void Update()
        {
            if (_playerBase == null || _enemyBase == null) return;

            if (!_playerBase.Health.IsDead && !_enemyBase.Health.IsDead)
            {
                _pTimer -= Time.deltaTime;
                if (_pTimer <= 0f) { SpawnAtPlayer(); _pTimer = playerSpawnInterval; }

                _eTimer -= Time.deltaTime;
                if (_eTimer <= 0f) { SpawnAtEnemy(); _eTimer = enemySpawnInterval; }
            }
        }

        // ---------- Spawn ----------
        private void SpawnAtPlayer()
        {
            bool ranged = Random.value < 0.5f;
            CreateUnit(ranged ? _pRanged : _pMelee, Team.Player, _enemyBase,
                ranged ? new Color(0.4f, 0.8f, 0.95f) : new Color(0.3f, 0.5f, 0.95f));
        }

        private void SpawnAtEnemy()
        {
            bool ranged = Random.value < 0.5f;
            CreateUnit(ranged ? _eRanged : _eMelee, Team.Enemy, _playerBase,
                ranged ? new Color(0.95f, 0.6f, 0.3f) : new Color(0.95f, 0.35f, 0.3f));
        }

        private void CreateUnit(UnitDefinition def, Team team, Base targetBase, Color color)
        {
            var go = new GameObject(def.displayName);
            float scale = def.attackType == AttackType.Ranged ? 0.45f : 0.55f;
            go.transform.localScale = Vector3.one * scale;
            AddSquare(go, color, sortingOrder: 1);

            go.AddComponent<Health>();
            var unit = go.AddComponent<Unit>();
            unit.laneOffset = new Vector3(0f, Random.Range(-0.7f, 0.7f), 0f);
            unit.Initialize(def, team, _path, targetBase);

            go.AddComponent<DemoUnitPulse>().Init(unit, go.transform.localScale);
        }

        // ---------- Dữ liệu (runtime) ----------
        private void BuildUnitDefs()
        {
            _pMelee = MakeUnit("Knight", UnitRole.Melee, AttackType.Melee,
                hp: 130, dmg: 14, range: 1.2f, interval: 0.8f, speed: 2.2f);
            _pRanged = MakeUnit("Archer", UnitRole.Ranged, AttackType.Ranged,
                hp: 70, dmg: 9, range: 4.0f, interval: 1.0f, speed: 2.0f, projectile: _arrowPrefab);

            _eMelee = MakeUnit("Orc", UnitRole.Melee, AttackType.Melee,
                hp: 130, dmg: 13, range: 1.2f, interval: 0.85f, speed: 2.1f);
            _eRanged = MakeUnit("Dark Mage", UnitRole.Ranged, AttackType.Ranged,
                hp: 65, dmg: 11, range: 3.6f, interval: 1.2f, speed: 1.9f, projectile: _boltPrefab);
        }

        private static UnitDefinition MakeUnit(string name, UnitRole role, AttackType atk,
            float hp, float dmg, float range, float interval, float speed, GameObject projectile = null)
        {
            var d = ScriptableObject.CreateInstance<UnitDefinition>();
            d.displayName = name; d.role = role; d.attackType = atk;
            d.maxHP = hp; d.attackDamage = dmg; d.attackRange = range;
            d.attackCooldown = interval; d.moveSpeed = speed;
            d.projectilePrefab = projectile; d.projectileSpeed = 9f;
            return d;
        }

        // ---------- Dựng cảnh ----------
        private void BuildPath()
        {
            var pathGO = new GameObject("PathSystem");
            _path = pathGO.AddComponent<PathSystem>();
            Vector2[] pts = { new(-8f, 0f), new(-3f, 0f), new(3f, 0f), new(8f, 0f) };
            var wps = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < pts.Length; i++)
            {
                var t = new GameObject($"WP_{i}").transform;
                t.SetParent(pathGO.transform);
                t.position = pts[i];
                wps.Add(t);
            }
            _path.SetWaypoints(wps);
        }

        private void BuildBases()
        {
            var pDef = ScriptableObject.CreateInstance<BaseDefinition>();
            pDef.displayName = "Player Base"; pDef.maxHP = baseHP;
            _playerBase = CreateBase("PlayerBase", Team.Player, pDef, _path.GetWaypoint(0),
                new Color(0.25f, 0.4f, 0.8f));

            var eDef = ScriptableObject.CreateInstance<BaseDefinition>();
            eDef.displayName = "Enemy Base"; eDef.maxHP = baseHP;
            _enemyBase = CreateBase("EnemyBase", Team.Enemy, eDef, _path.GetWaypoint(_path.Count - 1),
                new Color(0.8f, 0.3f, 0.3f));
        }

        private Base CreateBase(string n, Team team, BaseDefinition def, Vector3 pos, Color color)
        {
            var go = new GameObject(n);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 1.5f;
            AddSquare(go, color, sortingOrder: 0);
            go.AddComponent<Health>();
            var b = go.AddComponent<Base>();
            b.Configure(team, def, go.transform);
            return b;
        }

        private void BuildProjectiles()
        {
            _arrowPrefab = MakeProjectile("Arrow", new Color(1f, 0.9f, 0.2f));
            _boltPrefab = MakeProjectile("Bolt", new Color(0.8f, 0.4f, 1f));
        }

        private GameObject MakeProjectile(string n, Color color)
        {
            var go = new GameObject(n);
            go.transform.localScale = Vector3.one * 0.22f;
            AddSquare(go, color, sortingOrder: 2);
            go.AddComponent<Projectile>();
            go.SetActive(false); // khuôn; clone sẽ được bật khi bắn
            return go;
        }

        // ---------- Tiện ích hình ----------
        private static void BuildSquare()
        {
            if (_square != null) return;
            var tex = Texture2D.whiteTexture;
            _square = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), tex.width);
        }

        private static void AddSquare(GameObject go, Color color, int sortingOrder)
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _square; sr.color = color; sr.sortingOrder = sortingOrder;
        }

        private static void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.backgroundColor = new Color(0.10f, 0.10f, 0.12f);
        }

        private void OnGUI()
        {
            if (_playerBase == null || _enemyBase == null) return;
            var style = new GUIStyle(GUI.skin.label) { fontSize = 18 };
            GUI.Label(new Rect(10, 10, 500, 30),
                $"Player Base: {_playerBase.Health.CurrentHP:0}    Enemy Base: {_enemyBase.Health.CurrentHP:0}", style);
            if (_playerBase.Health.IsDead) GUI.Label(new Rect(10, 40, 500, 30), "PLAYER BASE DESTROYED", style);
            if (_enemyBase.Health.IsDead) GUI.Label(new Rect(10, 40, 500, 30), "ENEMY BASE DESTROYED (WIN)", style);

            // Nút test 4 hiệu ứng vùng — tác động lên quân ĐỊCH (đỏ) quanh giữa sân.
            var mid = Vector3.zero;
            if (GUI.Button(new Rect(10, 80, 130, 34), "Stun mid (2s)")) StunArea(mid, 3f, 2f);
            if (GUI.Button(new Rect(150, 80, 130, 34), "DoT mid")) SpawnDoT(mid, 3f, 25f, 4f);
            if (GUI.Button(new Rect(290, 80, 130, 34), "Meteor mid")) CastMeteor(mid, 3f, 250f, 1.5f);
            if (GUI.Button(new Rect(430, 80, 160, 34), "Execute <30% mid")) ExecuteArea(mid, 6f, 0.3f);
        }

        private void StunArea(Vector3 pos, float radius, float duration)
        {
            UnitRegistry.FindUnitsOfTeamInArea(Team.Enemy, pos, radius, _areaBuf);
            foreach (var u in _areaBuf) u.ApplyStun(duration);
        }

        private void SpawnDoT(Vector3 pos, float radius, float dps, float life)
        {
            var go = new GameObject("DoTZone");
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * (radius * 2f);
            AddSquare(go, new Color(1f, 0.45f, 0.1f, 0.25f), sortingOrder: -1);
            go.AddComponent<DamageZone>().Init(Team.Enemy, radius, dps, 0.5f, life);
        }

        private void CastMeteor(Vector3 pos, float radius, float damage, float delay)
        {
            var go = new GameObject("Meteor");
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * (radius * 2f);
            AddSquare(go, new Color(1f, 0.2f, 0.1f, 0.3f), sortingOrder: -1);
            go.AddComponent<MeteorStrike>().Init(Team.Enemy, radius, damage, delay);
        }

        private void ExecuteArea(Vector3 pos, float radius, float threshold)
        {
            UnitRegistry.FindUnitsOfTeamInArea(Team.Enemy, pos, radius, _areaBuf);
            foreach (var u in _areaBuf)
                if (u.Health.Normalized <= threshold) u.Health.TakeDamage(u.Health.CurrentHP);
        }
    }
}
