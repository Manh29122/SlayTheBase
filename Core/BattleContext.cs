using System.Collections.Generic;
using UnityEngine;
using SlayTheTower.Data;
using SlayTheTower.UI;

namespace SlayTheTower
{
    /// <summary>
    /// Bản hiện thực <see cref="IBattleContext"/>: nối hiệu ứng thẻ với các hệ thật.
    /// Phần energy/deck/hand đã hoạt động. Phần unit/base/zone sẽ hoàn thiện khi có
    /// hệ buff-runtime cho Unit (sau Phase 2) và zone (Phase 4) — hiện tạm log để theo dõi.
    /// </summary>
    public class BattleContext : MonoBehaviour, IBattleContext
    {
        [Header("Hệ thẻ / năng lượng")]
        [SerializeField] private EnergySystem energy;
        [SerializeField] private BattleDeck deck;
        [SerializeField] private HandView hand;

        [Header("Thế giới (tùy chọn — cho summon unit)")]
        [SerializeField] private PathSystem path;
        [SerializeField] private Base playerBase;
        [SerializeField] private Base enemyBase;
        [Tooltip("Prefab template cho unit KHÔNG có prefab riêng (vd unit người chơi tự tạo). " +
                 "Nên có SpriteRenderer (+ Unit/Health). Để trống = tạo GameObject trơn có SpriteRenderer.")]
        [SerializeField] private GameObject unitTemplatePrefab;
        [Tooltip("Camera để quy đổi điểm thả thẻ -> toạ độ thế giới (để trống = Camera.main).")]
        [SerializeField] private Camera worldCamera;

        private readonly List<Unit> _areaBuf = new();

        public Vector3 CastPosition { get; set; }

        public void SetWorldCamera(Camera cam) => worldCamera = cam;
        public void SetUnitTemplate(GameObject prefab) => unitTemplatePrefab = prefab;

        /// <summary>Quy đổi điểm thả (toạ độ màn hình) thành CastPosition (toạ độ thế giới).</summary>
        public void SetCastFromScreen(Vector2 screenPos, Camera uiCamera)
        {
            var cam = worldCamera != null ? worldCamera : Camera.main;
            if (cam == null) { CastPosition = Vector3.zero; return; }
            var w = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z)));
            w.z = 0f;
            CastPosition = w;
        }

        /// <summary>Gán nhanh tham chiếu bằng code (dùng cho demo/bootstrap).</summary>
        public void Configure(EnergySystem energySystem, BattleDeck battleDeck, HandView handView,
            PathSystem pathSystem, Base player, Base enemy)
        {
            energy = energySystem;
            deck = battleDeck;
            hand = handView;
            path = pathSystem;
            playerBase = player;
            enemyBase = enemy;
        }

        // ---------- Mana / energy ----------
        public void AddManaRegen(float perSecond) => energy?.AddRegen(perSecond);

        public void MultiplyCardCost(float multiplier)
        {
            deck?.MultiplyCost(multiplier);
            hand?.RefreshCostLabels();
        }

        public void AddEnergyNow(float amount) => energy?.AddEnergyNow(amount);
        public void AddMaxEnergy(float amount) => energy?.AddMaxEnergy(amount);

        // ---------- Deck / tay bài ----------
        public void DrawCards(int count) => hand?.DrawCards(count);
        public void ReturnRandomHandToDeckThenDraw(int r, int d) => hand?.ReturnRandomThenDraw(r, d);
        public void RecycleHandToBottomThenDraw(int d) => hand?.RecycleHandThenDraw(d);

        public void ChangeHandLimit(int delta, int min, int max)
        {
            if (deck == null) return;
            deck.ChangeHandLimit(delta, min, max);
            hand?.Refill(); // nếu tăng giới hạn thì rút thêm cho đầy
        }

        public void GrantFreeCards(int count) => deck?.GrantFreeCards(count);
        public void UpgradeRandomCards(int count) => hand?.UpgradeRandomCards(count);

        // ---------- Đơn vị (chờ hệ buff-runtime cho Unit) ----------
        public void HealAllPlayerUnits(float percentOfMax, float flat)
            => Debug.Log($"[Buff] Hồi {percentOfMax}% (+{flat}) máu toàn quân — chờ hệ unit runtime.");

        public void ApplyBuffToAllPlayerUnits(BuffDefinition buff)
            => Debug.Log($"[Buff] Áp '{(buff != null ? buff.displayName : "?")}' cho toàn quân — chờ hệ unit runtime.");

        public void AddOnDrawAttackBonus(float perDraw)
            => Debug.Log($"[Buff] +{perDraw} công mỗi lần rút bài — chờ hệ unit runtime.");

        public void SacrificeBaseToHealUnits(float baseDamagePercent, float unitHealPercent)
            => Debug.Log($"[Buff] Trừ {baseDamagePercent}% máu base, hồi {unitHealPercent}% máu quân — chờ hệ unit/base.");

        // ---------- Vùng (zone) ----------
        public void DealDamageToEnemiesInArea(Vector3 pos, float radius, float damage)
        {
            UnitRegistry.FindUnitsOfTeamInArea(Team.Enemy, pos, radius, _areaBuf);
            for (int i = 0; i < _areaBuf.Count; i++)
                _areaBuf[i].Health.TakeDamage(damage);
        }

        public void StunEnemiesInArea(Vector3 pos, float radius, float duration)
        {
            UnitRegistry.FindUnitsOfTeamInArea(Team.Enemy, pos, radius, _areaBuf);
            for (int i = 0; i < _areaBuf.Count; i++)
                _areaBuf[i].ApplyStun(duration);
        }

        public void ExecuteEnemiesInArea(Vector3 pos, float radius, float hpPercentThreshold)
        {
            UnitRegistry.FindUnitsOfTeamInArea(Team.Enemy, pos, radius, _areaBuf);
            for (int i = 0; i < _areaBuf.Count; i++)
            {
                var h = _areaBuf[i].Health;
                if (h.Normalized <= hpPercentThreshold) h.TakeDamage(h.CurrentHP);
            }
        }

        public void SpawnDamageOverTimeZone(Vector3 pos, float radius, float damagePerSecond, float duration)
        {
            var go = new GameObject("DoTZone");
            go.transform.position = pos;
            go.AddComponent<DamageZone>().Init(Team.Enemy, radius, damagePerSecond, 0.5f, duration);
        }

        public void SpawnMeteor(Vector3 pos, float radius, float damage, float delay)
        {
            var go = new GameObject("Meteor");
            go.transform.position = pos;
            go.AddComponent<MeteorStrike>().Init(Team.Enemy, radius, damage, delay);
        }

        // Hai vùng dưới cần hệ buff-runtime trên Unit (cờ miễn sát thương / hệ số tốc) — sẽ làm sau:
        public void SpawnAllyImmuneZone(Vector3 pos, float radius, float duration)
            => Debug.Log("[Buff] Vùng miễn sát thương cho quân ta — chờ cờ immune trên Unit/Health.");

        public void SpawnEnemySlowZone(Vector3 pos, float radius, float duration, float slowMultiplier)
            => Debug.Log("[Buff] Vùng làm chậm địch — chờ hệ số tốc runtime trên Unit.");

        // ---------- Triệu hồi (dùng hệ Phase 1) ----------
        public void SummonFromCard(CardDefinition def)
        {
            if (def == null || def.unitToSummon == null)
            {
                Debug.Log($"[Summon] {(def != null ? def.displayName : "?")} — chưa gán đơn vị để triệu hồi.");
                return;
            }
            if (path == null || playerBase == null || enemyBase == null)
            {
                Debug.Log($"[Summon] {def.displayName} x{def.summonCount} — chưa gắn thế giới (path/base).");
                return;
            }
            int bonus = deck != null ? deck.GetUpgrade(def) : 0;
            int total = Mathf.Max(1, def.summonCount + bonus);
            for (int i = 0; i < total; i++)
                SpawnUnit(def.unitToSummon);
        }

        private void SpawnUnit(UnitDefinition def)
        {
            GameObject go;
            if (def.prefab != null)
                go = Instantiate(def.prefab);
            else if (unitTemplatePrefab != null)
                go = Instantiate(unitTemplatePrefab);
            else
            {
                // Không có prefab/template -> GameObject trơn NHƯNG có SpriteRenderer để hoạt ảnh frame hiện được.
                go = new GameObject(string.IsNullOrEmpty(def.displayName) ? "Unit" : def.displayName);
                go.AddComponent<SpriteRenderer>();
            }
            go.SetActive(true);

            if (go.GetComponent<Health>() == null) go.AddComponent<Health>();
            var unit = go.GetComponent<Unit>();
            if (unit == null) unit = go.AddComponent<Unit>();
            unit.Initialize(def, Team.Player, path, enemyBase);
        }
    }
}
