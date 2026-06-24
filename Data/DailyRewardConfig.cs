using UnityEngine;

namespace SlayTheTower.Data
{
    /// <summary>
    /// Cấu hình phần thưởng điểm danh 7 ngày.
    /// Tạo asset qua: Assets > Create > Slay The Tower > Daily Reward Config.
    /// </summary>
    [CreateAssetMenu(fileName = "DailyRewardConfig", menuName = "Slay The Tower/Daily Reward Config", order = 21)]
    public class DailyRewardConfig : ScriptableObject
    {
        public enum RewardKind { Gold, Gem, Energy, Card }

        [System.Serializable]
        public struct DayReward
        {
            public RewardKind kind;
            [Min(0)] public int amount;
            [Tooltip("Dùng khi kind = Card.")]
            public CardDefinition card;
        }

        [Tooltip("Phần thưởng 7 ngày (index 0 = ngày 1).")]
        public DayReward[] days = new DayReward[7];
    }
}
