using UnityEngine;

namespace SlayTheTower.Data
{
    /// <summary>
    /// Cấu hình một wave: lịch spawn địch, thời lượng, máu base địch, phần thưởng.
    /// Tạo asset qua: Assets > Create > Slay The Tower > Wave.
    /// </summary>
    [CreateAssetMenu(fileName = "Wave_", menuName = "Slay The Tower/Wave", order = 4)]
    public class WaveDefinition : ScriptableObject
    {
        /// <summary>Một nhóm địch được spawn theo lịch.</summary>
        [System.Serializable]
        public struct EnemySpawn
        {
            [Tooltip("Loại đơn vị địch.")]
            public UnitDefinition unit;
            [Tooltip("Số lượng spawn trong nhóm này.")]
            [Min(1)] public int count;
            [Tooltip("Thời điểm bắt đầu spawn nhóm (giây tính từ đầu wave).")]
            [Min(0f)] public float startTime;
            [Tooltip("Khoảng cách giữa mỗi lần spawn trong nhóm (giây).")]
            [Min(0f)] public float interval;
        }

        [Header("Định danh")]
        public string displayName;
        [Tooltip("Số thứ tự wave (để hiển thị/sắp xếp).")]
        public int waveNumber = 1;

        [Header("Điều kiện")]
        [Tooltip("Cầm cự đủ số giây này là THẮNG wave (cách thắng 1).")]
        [Min(1f)] public float waveDuration = 60f;
        [Tooltip("Máu base địch trong wave này. Đánh về 0 cũng THẮNG (cách thắng 2).")]
        [Min(1f)] public float enemyBaseHP = 1000f;

        [Header("Lịch spawn địch")]
        public EnemySpawn[] spawns;

        [Header("Phần thưởng khi thắng")]
        [Tooltip("Vàng nhận được (nếu chọn vàng).")]
        [Min(0)] public int goldReward = 100;
        [Tooltip("Bảng rớt thẻ (nếu chọn thẻ).")]
        public DropTableDefinition cardRewardTable;
    }
}
