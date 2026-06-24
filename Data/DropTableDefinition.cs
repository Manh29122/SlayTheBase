using System.Collections.Generic;
using UnityEngine;

namespace SlayTheTower.Data
{
    /// <summary>
    /// Bảng rớt thẻ có trọng số — dùng cho phần thưởng sau wave & shop.
    /// Cho phép designer chỉnh "lá nào hay xuất hiện" bằng trọng số.
    /// Tạo asset qua: Assets > Create > Slay The Tower > Drop Table.
    /// </summary>
    [CreateAssetMenu(fileName = "DropTable_", menuName = "Slay The Tower/Drop Table", order = 5)]
    public class DropTableDefinition : ScriptableObject
    {
        [System.Serializable]
        public struct CardDrop
        {
            public CardDefinition card;
            [Tooltip("Trọng số xuất hiện. Càng cao càng hay ra.")]
            [Min(0f)] public float weight;
        }

        [Tooltip("Danh sách thẻ và trọng số tương ứng.")]
        public CardDrop[] entries;

        [Tooltip("Số lựa chọn thẻ hiển thị cho người chơi sau wave.")]
        [Min(1)] public int choicesToOffer = 3;

        /// <summary>Bốc ngẫu nhiên 1 thẻ theo trọng số.</summary>
        public CardDefinition Roll()
        {
            if (entries == null || entries.Length == 0)
                return null;

            float total = 0f;
            foreach (var e in entries)
                total += Mathf.Max(0f, e.weight);

            // Nếu mọi trọng số = 0 thì bốc đều.
            if (total <= 0f)
                return entries[Random.Range(0, entries.Length)].card;

            float r = Random.value * total;
            foreach (var e in entries)
            {
                r -= Mathf.Max(0f, e.weight);
                if (r <= 0f)
                    return e.card;
            }
            return entries[entries.Length - 1].card;
        }

        /// <summary>
        /// Bốc <paramref name="count"/> thẻ KHÁC NHAU (không trùng) để người chơi chọn.
        /// Nếu không đủ thẻ khác nhau thì trả về tối đa có thể.
        /// </summary>
        public List<CardDefinition> RollDistinct(int count)
        {
            var result = new List<CardDefinition>();
            if (entries == null || entries.Length == 0)
                return result;

            // Sao chép pool để loại dần thẻ đã bốc.
            var pool = new List<CardDrop>(entries);
            while (result.Count < count && pool.Count > 0)
            {
                float total = 0f;
                foreach (var e in pool)
                    total += Mathf.Max(0f, e.weight);

                int pickIndex = pool.Count - 1;
                if (total > 0f)
                {
                    float r = Random.value * total;
                    for (int i = 0; i < pool.Count; i++)
                    {
                        r -= Mathf.Max(0f, pool[i].weight);
                        if (r <= 0f) { pickIndex = i; break; }
                    }
                }
                else
                {
                    pickIndex = Random.Range(0, pool.Count);
                }

                if (pool[pickIndex].card != null)
                    result.Add(pool[pickIndex].card);
                pool.RemoveAt(pickIndex);
            }
            return result;
        }
    }
}
