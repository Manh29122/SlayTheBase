using UnityEngine;
using SlayTheTower.Data;

namespace SlayTheTower
{
    /// <summary>
    /// "Seam" giữa hiệu ứng thẻ (data) và các hệ thống trận đấu (runtime).
    /// Các hệ thống energy / deck / unit / zone của trận sẽ hiện thực interface này;
    /// <see cref="CardEffectResolver"/> chỉ gọi qua đây nên hiệu ứng không phụ thuộc cài đặt cụ thể.
    /// (Bản hiện thực đầy đủ sẽ làm cùng Phase 3 - energy/deck và Phase 4 - zone.)
    /// </summary>
    public interface IBattleContext
    {
        /// <summary>Vị trí (world) nơi thẻ được thả — dùng để đặt vùng (zone).</summary>
        Vector3 CastPosition { get; }

        // ----- Mana / energy -----
        void AddManaRegen(float perSecond);
        void MultiplyCardCost(float multiplier);
        void AddEnergyNow(float amount);
        void AddMaxEnergy(float amount);

        // ----- Đơn vị quân -----
        void HealAllPlayerUnits(float percentOfMax, float flat);
        void ApplyBuffToAllPlayerUnits(BuffDefinition buff);
        void AddOnDrawAttackBonus(float perDraw);

        // ----- Base -----
        void SacrificeBaseToHealUnits(float baseDamagePercent, float unitHealPercent);

        // ----- Deck / tay bài -----
        void DrawCards(int count);
        void ReturnRandomHandToDeckThenDraw(int returnCount, int drawCount);
        void RecycleHandToBottomThenDraw(int drawCount);
        void ChangeHandLimit(int delta, int min, int max);
        void GrantFreeCards(int count);
        void UpgradeRandomCards(int count);

        // ----- Vùng (zone) -----
        void SpawnAllyImmuneZone(Vector3 pos, float radius, float duration);
        void DealDamageToEnemiesInArea(Vector3 pos, float radius, float damage);
        void SpawnEnemySlowZone(Vector3 pos, float radius, float duration, float slowMultiplier);
        void StunEnemiesInArea(Vector3 pos, float radius, float duration);
        void SpawnDamageOverTimeZone(Vector3 pos, float radius, float damagePerSecond, float duration);
        void SpawnMeteor(Vector3 pos, float radius, float damage, float delay);
        void ExecuteEnemiesInArea(Vector3 pos, float radius, float hpPercentThreshold);
    }
}
