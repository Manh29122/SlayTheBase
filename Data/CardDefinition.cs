using System.Collections.Generic;
using UnityEngine;

namespace SlayTheTower.Data
{
    /// <summary>
    /// Định nghĩa một lá bài trong deck (triệu hồi quân hoặc kích hoạt buff).
    /// Tạo asset qua: Assets > Create > Slay The Tower > Card.
    /// </summary>
    [CreateAssetMenu(fileName = "Card_", menuName = "Slay The Tower/Card", order = 3)]
    public class CardDefinition : ScriptableObject
    {
        [Header("Định danh")]
        public string id;
        public string displayName;
        [TextArea] public string description;
        [Tooltip("Hình minh họa lá bài.")]
        public Sprite artwork;
        public Rarity rarity = Rarity.Common;

        [Header("Prefab")]
        [Tooltip("Prefab UI của lá (có CardView). HandView dùng để spawn khi rút bài. " +
                 "Để trống = dùng prefab mặc định của HandView.\n" +
                 "Nền/khung/màu chữ tự chỉnh trong prefab — CardView KHÔNG ghi đè.")]
        public GameObject cardPrefab;

        [Header("Chi phí")]
        [Tooltip("Energy cần để đánh thẻ này trong trận.")]
        [Min(0)] public int energyCost = 3;
        [Tooltip("Giá mua trong shop (bằng vàng).")]
        [Min(0)] public int shopPrice = 50;

        [Header("Nội dung thẻ")]
        public CardType cardType = CardType.SummonUnit;

        [Header("→ Khi là thẻ Triệu hồi (SummonUnit)")]
        [Tooltip("Đơn vị được triệu hồi.")]
        public UnitDefinition unitToSummon;
        [Tooltip("Số lượng đơn vị spawn ra mỗi lần đánh thẻ.")]
        [Min(1)] public int summonCount = 1;

        [Header("→ Khi là thẻ Buff / hiệu ứng")]
        [Tooltip("Danh sách hiệu ứng kích hoạt khi đánh thẻ (một thẻ có thể gồm nhiều hiệu ứng).")]
        public List<CardEffectDefinition> effects = new();
    }
}
