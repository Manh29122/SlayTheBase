using UnityEngine;

namespace SlayTheTower.Data
{
    /// <summary>
    /// Một hiệu ứng đơn lẻ mà thẻ bài kích hoạt. Chọn <see cref="effectType"/> rồi điền
    /// các tham số liên quan (field nào không liên quan tới loại đang chọn thì bỏ qua).
    /// Mỗi hiệu ứng là 1 asset độc lập, có thể tái dùng cho nhiều thẻ.
    /// Tạo asset qua: Assets > Create > Slay The Tower > Card Effect.
    /// </summary>
    [CreateAssetMenu(fileName = "Effect_", menuName = "Slay The Tower/Card Effect", order = 6)]
    public class CardEffectDefinition : ScriptableObject
    {
        [Header("Định danh")]
        public string id;
        public string displayName;
        [TextArea] public string description;

        [Header("Loại hiệu ứng")]
        public CardEffectType effectType = CardEffectType.UnitStatBuff;

        [Header("UnitStatBuff")]
        [Tooltip("BuffDefinition áp cho toàn quân (công/thủ/tốc/thần thánh...).")]
        public BuffDefinition unitBuff;

        [Header("Tham số chung")]
        [Tooltip("Giá trị CHÍNH, ý nghĩa theo loại:\n" +
                 "• ManaRegenAdd: mana/giây\n" +
                 "• InstantHealUnits: % máu tối đa hồi\n" +
                 "• ZoneDamageEnemy: sát thương\n" +
                 "• ZoneSlowEnemy: hệ số tốc địch (vd 0.5 = còn 50%)\n" +
                 "• OnDrawAttackBuff: +công mỗi lần rút\n" +
                 "• BaseSacrificeHeal: % máu base bị trừ\n" +
                 "• EnergyCostMultiplier: hệ số nhân chi phí (vd 1.5)")]
        public float amount;

        [Tooltip("Giá trị PHỤ, ý nghĩa theo loại:\n" +
                 "• InstantHealUnits: lượng hồi cộng thẳng (flat)\n" +
                 "• BaseSacrificeHeal: % máu hồi cho quân")]
        public float secondaryAmount;

        [Tooltip("Số lượng CHÍNH:\n" +
                 "• DrawCards: số lá rút\n" +
                 "• ReturnRandomAndDraw: số lá trả về deck\n" +
                 "• RecycleHandDrawTop: số lá rút lại")]
        public int count = 1;

        [Tooltip("Số lượng PHỤ:\n• ReturnRandomAndDraw: số lá rút sau khi trả.")]
        public int secondaryCount = 1;

        [Header("Vùng (Zone*)")]
        public float radius = 2f;
        public float zoneDuration = 3f;

        [Header("Giới hạn tay (HandLimitDelta)")]
        public int handLimitDelta = 1;
        [Tooltip("Chặn dưới của giới hạn tay.")]
        public int handLimitMin = 3;
        [Tooltip("Chặn trên của giới hạn tay.")]
        public int handLimitMax = 7;
    }
}
