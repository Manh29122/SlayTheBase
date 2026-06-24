using UnityEngine;

namespace SlayTheTower.Data
{
    /// <summary>
    /// Định nghĩa dữ liệu cho công trình (base) của người chơi hoặc địch.
    /// Tạo asset qua: Assets > Create > Slay The Tower > Base.
    /// </summary>
    [CreateAssetMenu(fileName = "Base_", menuName = "Slay The Tower/Base", order = 1)]
    public class BaseDefinition : ScriptableObject
    {
        [Header("Định danh")]
        public string displayName;
        public Sprite icon;
        [Tooltip("Prefab công trình hiển thị trên sân.")]
        public GameObject prefab;

        [Header("Chỉ số")]
        [Min(1f)] public float maxHP = 1000f;

        [Header("Phòng thủ (tùy chọn)")]
        [Tooltip("Base có tự bắn đơn vị địch trong tầm không?")]
        public bool canAttack = false;
        [Min(0f)] public float attackDamage = 0f;
        [Min(0f)] public float attackRange = 0f;
        [Min(0.05f)] public float attackInterval = 1f;
        [Tooltip("Prefab đạn nếu base có tấn công tầm xa.")]
        public GameObject projectilePrefab;
    }
}
