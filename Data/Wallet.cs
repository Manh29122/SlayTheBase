using System;
using UnityEngine;

namespace SlayTheTower.Data
{
    /// <summary>
    /// Ví VÀNG của người chơi (lưu PlayerPrefs). Dùng để mua thẻ trong shop; bán thẻ được hoàn vàng.
    /// </summary>
    public static class Wallet
    {
        private const string Key = "wallet_gold";

        /// <summary>Bắn ra số vàng MỚI mỗi khi thay đổi (để UI cập nhật).</summary>
        public static event Action<int> OnGoldChanged;

        public static int Gold => PlayerPrefs.GetInt(Key, 0);

        /// <summary>Cộng (hoặc trừ nếu âm) vàng.</summary>
        public static void Add(int amount)
        {
            if (amount == 0) return;
            SetGold(Gold + amount);
        }

        /// <summary>Tiêu vàng nếu đủ. Trả về false nếu không đủ.</summary>
        public static bool TrySpend(int amount)
        {
            if (amount < 0 || Gold < amount) return false;
            SetGold(Gold - amount);
            return true;
        }

        /// <summary>Đặt thẳng số vàng (vd thưởng/cheat/test).</summary>
        public static void SetGold(int value)
        {
            PlayerPrefs.SetInt(Key, Mathf.Max(0, value));
            PlayerPrefs.Save();
            OnGoldChanged?.Invoke(Gold);
        }
    }
}
