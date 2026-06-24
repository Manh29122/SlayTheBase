using UnityEngine;

namespace SlayTheTower.Data
{
    /// <summary>
    /// Định nghĩa một hiệu ứng buff (tạm thời hoặc tức thời).
    /// Dùng cho thẻ buff và aura của đơn vị hỗ trợ.
    /// Tạo asset qua: Assets > Create > Slay The Tower > Buff.
    /// </summary>
    [CreateAssetMenu(fileName = "Buff_", menuName = "Slay The Tower/Buff", order = 2)]
    public class BuffDefinition : ScriptableObject
    {
        [Header("Định danh")]
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Phạm vi & thời gian")]
        public BuffScope scope = BuffScope.AllAllyUnits;
        [Tooltip("Bán kính (chỉ dùng cho scope AreaAroundCast).")]
        [Min(0f)] public float radius = 3f;
        [Tooltip("Thời gian hiệu lực (giây). Để 0 nếu là hiệu ứng tức thời (vd hồi máu một lần).")]
        [Min(0f)] public float duration = 5f;

        [Header("Hệ số khi đang hiệu lực (1 = không đổi)")]
        [Tooltip("Nhân vào sát thương gây ra.")]
        [Min(0f)] public float attackMultiplier = 1f;
        [Tooltip("Cộng thẳng vào sát thương (flat). Vd +5.")]
        public float attackFlatBonus = 0f;
        [Tooltip("% giảm sát thương nhận vào (0..1). Vd 0.3 = giảm 30%.")]
        [Range(0f, 1f)] public float damageReductionPercent = 0f;
        [Tooltip("Nhân vào tốc độ di chuyển.")]
        [Min(0f)] public float moveSpeedMultiplier = 1f;
        [Tooltip("Nhân vào tốc độ đánh (>1 = đánh nhanh hơn).")]
        [Min(0f)] public float attackSpeedMultiplier = 1f;
        [Tooltip("Nhân vào MÁU TỐI ĐA (vd 2 = +100%). Dùng cho trạng thái 'thần thánh'.")]
        [Min(0f)] public float maxHpMultiplier = 1f;

        [Header("Hồi máu")]
        [Tooltip("Hồi máu tức thời (1 lần) khi áp dụng.")]
        [Min(0f)] public float instantHeal = 0f;
        [Tooltip("Hồi máu mỗi giây trong suốt thời gian hiệu lực.")]
        [Min(0f)] public float healPerSecond = 0f;

        [Header("Năng lượng")]
        [Tooltip("Cộng thêm tốc độ hồi energy (energy/giây) — dùng cho aura đơn vị 'tăng năng lượng'.")]
        public float bonusEnergyRegen = 0f;

        /// <summary>Buff tức thời (không kéo dài theo thời gian).</summary>
        public bool IsInstant => duration <= 0f;
    }
}
