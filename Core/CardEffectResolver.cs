using System.Collections.Generic;
using SlayTheTower.Data;

namespace SlayTheTower
{
    /// <summary>
    /// Chuyển một (hoặc nhiều) <see cref="CardEffectDefinition"/> thành lời gọi cụ thể
    /// trên <see cref="IBattleContext"/>. Đây là nơi duy nhất "dịch" loại hiệu ứng -> hành động.
    /// </summary>
    public static class CardEffectResolver
    {
        public static void Execute(CardEffectDefinition e, IBattleContext ctx)
        {
            if (e == null || ctx == null) return;

            switch (e.effectType)
            {
                case CardEffectType.ManaRegenAdd:
                    ctx.AddManaRegen(e.amount);
                    break;

                case CardEffectType.InstantHealUnits:
                    ctx.HealAllPlayerUnits(e.amount, e.secondaryAmount);
                    break;

                case CardEffectType.UnitStatBuff:
                    ctx.ApplyBuffToAllPlayerUnits(e.unitBuff);
                    break;

                case CardEffectType.ZoneAllyImmune:
                    ctx.SpawnAllyImmuneZone(ctx.CastPosition, e.radius, e.zoneDuration);
                    break;

                case CardEffectType.ZoneDamageEnemy:
                    ctx.DealDamageToEnemiesInArea(ctx.CastPosition, e.radius, e.amount);
                    break;

                case CardEffectType.ZoneSlowEnemy:
                    ctx.SpawnEnemySlowZone(ctx.CastPosition, e.radius, e.zoneDuration, e.amount);
                    break;

                case CardEffectType.DrawCards:
                    ctx.DrawCards(e.count);
                    break;

                case CardEffectType.ReturnRandomAndDraw:
                    ctx.ReturnRandomHandToDeckThenDraw(e.count, e.secondaryCount);
                    break;

                case CardEffectType.RecycleHandDrawTop:
                    ctx.RecycleHandToBottomThenDraw(e.count);
                    break;

                case CardEffectType.OnDrawAttackBuff:
                    ctx.AddOnDrawAttackBonus(e.amount);
                    break;

                case CardEffectType.BaseSacrificeHeal:
                    ctx.SacrificeBaseToHealUnits(e.amount, e.secondaryAmount);
                    break;

                case CardEffectType.EnergyCostMultiplier:
                    ctx.MultiplyCardCost(e.amount);
                    break;

                case CardEffectType.HandLimitDelta:
                    ctx.ChangeHandLimit(e.handLimitDelta, e.handLimitMin, e.handLimitMax);
                    break;

                case CardEffectType.EnergyBurst:
                    ctx.AddEnergyNow(e.amount);
                    break;

                case CardEffectType.MaxEnergyAdd:
                    ctx.AddMaxEnergy(e.amount);
                    break;

                case CardEffectType.NextCardFree:
                    ctx.GrantFreeCards(e.count);
                    break;

                case CardEffectType.UpgradeRandomCard:
                    ctx.UpgradeRandomCards(e.count);
                    break;

                case CardEffectType.ZoneStun:
                    ctx.StunEnemiesInArea(ctx.CastPosition, e.radius, e.zoneDuration);
                    break;

                case CardEffectType.ZoneDamageOverTime:
                    ctx.SpawnDamageOverTimeZone(ctx.CastPosition, e.radius, e.amount, e.zoneDuration);
                    break;

                case CardEffectType.Meteor:
                    ctx.SpawnMeteor(ctx.CastPosition, e.radius, e.amount, e.zoneDuration);
                    break;

                case CardEffectType.ExecuteEnemies:
                    ctx.ExecuteEnemiesInArea(ctx.CastPosition, e.radius, e.amount / 100f);
                    break;
            }
        }

        public static void ExecuteAll(IEnumerable<CardEffectDefinition> effects, IBattleContext ctx)
        {
            if (effects == null) return;
            foreach (var e in effects)
                Execute(e, ctx);
        }
    }
}
