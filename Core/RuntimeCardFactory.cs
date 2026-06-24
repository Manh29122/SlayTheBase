using System;
using UnityEngine;
using SlayTheTower.Data;
using SlayTheTower.Drawing;

namespace SlayTheTower
{
    /// <summary>
    /// Dựng một <see cref="CardDefinition"/> TRONG RAM từ <see cref="PlayerCardData"/> (thẻ người chơi vẽ).
    /// Dùng được trên build mobile: ScriptableObject.CreateInstance chạy lúc runtime, KHÔNG tạo asset/prefab.
    /// Visual lấy từ 1 prefab template ship sẵn; ảnh nạp từ PNG đã lưu.
    /// </summary>
    public static class RuntimeCardFactory
    {
        /// <param name="templatePrefab">Prefab thẻ dùng chung (có CardView), ship sẵn trong app.</param>
        /// <param name="unitLookup">Hàm tra UnitDefinition theo id (cho thẻ triệu hồi).</param>
        public static CardDefinition Build(PlayerCardData d, GameObject templatePrefab,
            Func<string, UnitDefinition> unitLookup = null)
        {
            var c = ScriptableObject.CreateInstance<CardDefinition>();
            c.id = d.id;
            c.displayName = d.displayName;
            c.description = d.description;
            c.energyCost = d.energyCost;
            c.shopPrice = d.shopPrice;
            c.rarity = d.rarity;
            c.cardType = d.cardType;
            c.summonCount = Mathf.Max(1, d.summonCount);
            c.cardPrefab = templatePrefab;

            if (!string.IsNullOrEmpty(d.artFile))
                c.artwork = CardArtStorage.Load(d.artFile);

            if (d.cardType == CardType.SummonUnit && unitLookup != null && !string.IsNullOrEmpty(d.unitId))
                c.unitToSummon = unitLookup(d.unitId);

            return c;
        }
    }
}
