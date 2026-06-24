using System.Collections.Generic;
using UnityEngine;

namespace SlayTheTower.Data
{
    /// <summary>
    /// Bộ bài khởi đầu: danh sách thẻ + số lượng mỗi loại + giới hạn tay ban đầu.
    /// Tạo asset qua: Assets > Create > Slay The Tower > Deck.
    /// </summary>
    [CreateAssetMenu(fileName = "Deck_", menuName = "Slay The Tower/Deck", order = 7)]
    public class DeckDefinition : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            public CardDefinition card;
            [Min(1)] public int count;
        }

        [Tooltip("Các thẻ trong bộ bài khởi đầu và số lượng mỗi loại.")]
        public Entry[] startingCards;

        [Tooltip("Số lá tối đa trên tay lúc bắt đầu (mặc định 4).")]
        [Min(1)] public int startingHandLimit = 4;

        /// <summary>Bung thành danh sách phẳng (mỗi count = 1 phần tử).</summary>
        public List<CardDefinition> BuildCardList()
        {
            var list = new List<CardDefinition>();
            if (startingCards == null) return list;
            foreach (var e in startingCards)
            {
                if (e.card == null) continue;
                for (int i = 0; i < Mathf.Max(1, e.count); i++)
                    list.Add(e.card);
            }
            return list;
        }
    }
}
