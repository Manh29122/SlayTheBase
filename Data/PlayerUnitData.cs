namespace SlayTheTower.Data
{
    /// <summary>
    /// Dữ liệu một UNIT do người chơi tạo (mobile) — plain class để JsonUtility lưu được.
    /// Frame hoạt ảnh lưu theo DANH SÁCH id ảnh (PNG trong CardArtStorage), không lưu sprite trực tiếp.
    /// </summary>
    [System.Serializable]
    public class PlayerUnitData
    {
        public string id;
        public string displayName;

        // ----- Chỉ số -----
        public float maxHP = 100f;
        public float moveSpeed = 2f;
        public AttackType attackType = AttackType.Melee;
        public float attackDamage = 10f;
        public float attackRange = 1.5f;
        [UnityEngine.Tooltip("Thời gian chờ giữa 2 đòn (giây).")]
        public float attackCooldown = 0.5f;

        // ----- Hoạt ảnh ĐI BỘ -----
        [UnityEngine.Tooltip("Danh sách id ảnh (theo thứ tự) cho anim đi bộ.")]
        public string[] walkFrameIds;
        public float walkDuration = 0.6f;

        // ----- Hoạt ảnh TẤN CÔNG -----
        [UnityEngine.Tooltip("Danh sách id ảnh (theo thứ tự) cho anim tấn công.")]
        public string[] attackFrameIds;
        public float attackDuration = 0.4f;
        [UnityEngine.Tooltip("Frame (0-based) gây sát thương (cận chiến) / spawn đạn (tầm xa).")]
        public int attackHitFrame = 1;

        // ----- Đạn (chỉ tầm xa) -----
        [UnityEngine.Tooltip("Id ảnh viên đạn (nếu attackType = Ranged).")]
        public string projectileFrameId;
        public float projectileSpeed = 8f;
    }
}
