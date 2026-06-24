using System;
using UnityEngine;

namespace SlayTheTower.Data
{
    /// <summary>
    /// Logic mua/bán thẻ: mua tốn vàng (<see cref="Wallet"/>) + cộng vào kho (<see cref="CardInventory"/>);
    /// bán bớt 1 lá để hoàn lại 1 phần vàng.
    /// </summary>
    public static class ShopSystem
    {
        /// <summary>Tỉ lệ hoàn vàng khi bán (0.5 = bán lại được 50% giá mua).</summary>
        public static float SellRefundPercent = 0.5f;

        /// <summary>Bắn ra sau mỗi giao dịch mua/bán thành công.</summary>
        public static event Action OnTransaction;

        public static int SellValue(int price) => Mathf.RoundToInt(price * SellRefundPercent);

        public static bool TryBuy(string cardId, int price)
        {
            if (string.IsNullOrEmpty(cardId) || price < 0) return false;
            if (!Wallet.TrySpend(price)) return false; // không đủ vàng
            CardInventory.Add(cardId, 1);
            OnTransaction?.Invoke();
            return true;
        }

        public static bool TrySell(string cardId, int price)
        {
            if (!CardInventory.TryRemove(cardId, 1)) return false; // không có lá để bán
            Wallet.Add(SellValue(price));
            OnTransaction?.Invoke();
            return true;
        }

        // ----- Tiện ích cho 2 loại thẻ -----
        public static bool TryBuy(CardDefinition c) => c != null && TryBuy(c.id, c.shopPrice);
        public static bool TrySell(CardDefinition c) => c != null && TrySell(c.id, c.shopPrice);
        public static bool TryBuy(PlayerCardData c) => c != null && TryBuy(c.id, c.shopPrice);
        public static bool TrySell(PlayerCardData c) => c != null && TrySell(c.id, c.shopPrice);
    }
}
