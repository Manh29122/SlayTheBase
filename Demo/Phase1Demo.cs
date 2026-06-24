using System.Collections.Generic;
using UnityEngine;
using SlayTheTower.Data;

namespace SlayTheTower
{
    /// <summary>
    /// BOOTSTRAP DEMO cho Phase 1 — tự dựng toàn bộ scene bằng code:
    /// path (zig-zag) + 2 base + spawn quân Player đi đập base địch.
    ///
    /// Cách dùng: tạo 1 GameObject rỗng trong SampleScene, gắn script này, bấm Play.
    /// Không cần kéo thả gì. Có thể xoá script/GameObject này khi sang phase sau.
    /// </summary>
    public class Phase1Demo : MonoBehaviour
    {
        [Header("Cấu hình demo")]
        [Tooltip("Giây giữa mỗi lần spawn quân Player.")]
        [SerializeField] private float spawnInterval = 1.5f;
        [Tooltip("Sát thương mỗi đòn của quân Player.")]
        [SerializeField] private float playerUnitDamage = 25f;
        [Tooltip("Máu base địch (đánh về 0 = thắng demo).")]
        [SerializeField] private float enemyBaseHP = 500f;

        private PathSystem _path;
        private Base _enemyBase;
        private UnitDefinition _soldierDef;
        private float _spawnTimer;

        private static Sprite _squareSprite;

        private void Start()
        {
            BuildSquareSprite();
            SetupCamera();
            BuildPath();
            BuildBases();
            BuildUnitDefinition();
        }

        private void Update()
        {
            if (_enemyBase == null || _enemyBase.Health.IsDead) return;

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                SpawnPlayerUnit();
                _spawnTimer = spawnInterval;
            }
        }

        // ---------- Dựng scene ----------

        private static void BuildSquareSprite()
        {
            if (_squareSprite != null) return;
            var tex = Texture2D.whiteTexture;
            _squareSprite = Sprite.Create(
                tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), tex.width);
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

        private void BuildPath()
        {
            var pathGO = new GameObject("PathSystem");
            _path = pathGO.AddComponent<PathSystem>();

            Vector2[] points =
            {
                new(-7f, 0f), new(-3f, 2.5f), new(3f, -2.5f), new(7f, 0f)
            };

            var wps = new List<Transform>();
            for (int i = 0; i < points.Length; i++)
            {
                var wp = new GameObject($"WP_{i}").transform;
                wp.SetParent(pathGO.transform);
                wp.position = points[i];
                wps.Add(wp);
            }
            _path.SetWaypoints(wps);
        }

        private void BuildBases()
        {
            var playerDef = ScriptableObject.CreateInstance<BaseDefinition>();
            playerDef.displayName = "Player Base";
            playerDef.maxHP = 1000f;
            CreateBase("PlayerBase", Team.Player, playerDef,
                _path.GetWaypoint(0), new Color(0.30f, 0.60f, 1f));

            var enemyDef = ScriptableObject.CreateInstance<BaseDefinition>();
            enemyDef.displayName = "Enemy Base";
            enemyDef.maxHP = enemyBaseHP;
            _enemyBase = CreateBase("EnemyBase", Team.Enemy, enemyDef,
                _path.GetWaypoint(_path.Count - 1), new Color(1f, 0.40f, 0.40f));
        }

        private Base CreateBase(string goName, Team team, BaseDefinition def, Vector3 pos, Color color)
        {
            var go = new GameObject(goName);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 1.5f;
            AddSquareVisual(go, color, sortingOrder: 0);

            go.AddComponent<Health>();
            var b = go.AddComponent<Base>();
            b.Configure(team, def, go.transform);
            return b;
        }

        private void BuildUnitDefinition()
        {
            _soldierDef = ScriptableObject.CreateInstance<UnitDefinition>();
            _soldierDef.id = "demo_soldier";
            _soldierDef.displayName = "Soldier";
            _soldierDef.maxHP = 100f;
            _soldierDef.moveSpeed = 2.5f;
            _soldierDef.role = UnitRole.Melee;
            _soldierDef.attackType = AttackType.Melee;
            _soldierDef.attackDamage = playerUnitDamage;
            _soldierDef.attackRange = 1.2f;
            _soldierDef.attackCooldown = 0.8f;
        }

        private void SpawnPlayerUnit()
        {
            var go = new GameObject("PlayerUnit");
            go.transform.localScale = Vector3.one * 0.5f;
            AddSquareVisual(go, new Color(0.20f, 0.90f, 0.40f), sortingOrder: 1);

            go.AddComponent<Health>();
            var unit = go.AddComponent<Unit>();
            unit.Initialize(_soldierDef, Team.Player, _path, _enemyBase);
        }

        private static void AddSquareVisual(GameObject go, Color color, int sortingOrder)
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _squareSprite;
            sr.color = color;
            sr.sortingOrder = sortingOrder;
        }

        // ---------- HUD debug ----------

        private void OnGUI()
        {
            if (_enemyBase == null) return;

            var style = new GUIStyle(GUI.skin.label) { fontSize = 18 };
            GUI.Label(new Rect(10, 10, 500, 30),
                $"Enemy Base HP: {_enemyBase.Health.CurrentHP:0} / {_enemyBase.Health.MaxHP:0}", style);

            if (_enemyBase.Health.IsDead)
                GUI.Label(new Rect(10, 40, 500, 30), ">>> ENEMY BASE DESTROYED! (WIN) <<<", style);
        }
    }
}
