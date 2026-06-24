using UnityEngine;

namespace SlayTheTower.Data
{
    /// <summary>
    /// Định nghĩa dữ liệu (chỉ số) cho một loại đơn vị quân.
    /// Tạo asset qua: Assets > Create > Slay The Tower > Unit.
    /// </summary>
    [CreateAssetMenu(fileName = "Unit_", menuName = "Slay The Tower/Unit", order = 0)]
    public class UnitDefinition : ScriptableObject
    {
        [Header("Định danh")]
        [Tooltip("Mã định danh duy nhất, vd: knight, archer...")]
        public string id;
        [Tooltip("Tên hiển thị cho người chơi.")]
        public string displayName;
        [TextArea] public string description;
        [Tooltip("Icon hiển thị trên thẻ/HUD.")]
        public Sprite icon;
        [Tooltip("Prefab sẽ được spawn ra sân khi triệu hồi đơn vị này.")]
        public GameObject prefab;

        [Header("Chỉ số cơ bản")]
        [Min(1f)] public float maxHP = 100f;
        [Tooltip("Tốc độ di chuyển dọc tuyến đường (đơn vị/giây).")]
        [Min(0f)] public float moveSpeed = 2f;

        [Header("Tấn công")]
        public UnitRole role = UnitRole.Melee;
        public AttackType attackType = AttackType.Melee;
        [Tooltip("Sát thương mỗi đòn. Với Healer, đây là lượng máu hồi mỗi nhịp.")]
        [Min(0f)] public float attackDamage = 10f;
        [Tooltip("Tầm phát hiện & tấn công mục tiêu.")]
        [Min(0f)] public float attackRange = 1.5f;
        [Tooltip("Thời gian CHỜ giữa 2 đòn (sau khi anim đánh xong), giây. Vd 0.5.")]
        [Min(0f)] public float attackCooldown = 0.5f;
        [Tooltip("Prefab đạn/phép (chỉ dùng cho đơn vị tầm xa).")]
        public GameObject projectilePrefab;
        [Tooltip("Tốc độ bay của đạn/phép (đơn vị/giây).")]
        [Min(0.1f)] public float projectileSpeed = 8f;

        [Header("Hoạt ảnh (đổi sprite theo frame — KHÔNG dùng Animator)")]
        [Tooltip("Các frame ĐI BỘ (phát lặp).")]
        public Sprite[] walkFrames;
        [Tooltip("Thời lượng 1 vòng đi bộ (giây).")]
        [Min(0.05f)] public float walkDuration = 0.6f;
        [Tooltip("Các frame TẤN CÔNG (phát 1 lần mỗi đòn).")]
        public Sprite[] attackFrames;
        [Tooltip("Thời lượng 1 đòn đánh (giây).")]
        [Min(0.05f)] public float attackDuration = 0.4f;
        [Tooltip("Frame (0-based) trong attackFrames mà: CẬN CHIẾN gây sát thương / TẦM XA spawn đạn.")]
        [Min(0)] public int attackHitFrame = 1;

        [Header("Hỗ trợ / Aura (chỉ cho role Support)")]
        [Tooltip("Buff phát ra cho đồng minh quanh đơn vị (để trống nếu không có).")]
        public BuffDefinition auraBuff;
        [Tooltip("Bán kính aura.")]
        [Min(0f)] public float auraRadius = 0f;
    }
}
