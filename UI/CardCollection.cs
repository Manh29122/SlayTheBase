using System.Collections.Generic;
using UnityEngine;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Bộ sưu tập lá bài: danh sách prefab <see cref="CardView"/> để gom thành 1 asset
    /// (thay vì kéo list trực tiếp trong Inspector). Kéo vào ô "Collection" của CardHandView.
    /// Tạo asset qua: Assets > Create > Slay The Tower > Card Collection.
    /// </summary>
    [CreateAssetMenu(fileName = "CardCollection_", menuName = "Slay The Tower/Card Collection", order = 10)]
    public class CardCollection : ScriptableObject
    {
        [Tooltip("Các prefab lá bài (mỗi prefab có CardView).")]
        [SerializeField] private List<CardView> cards = new();

        public IReadOnlyList<CardView> Cards => cards;
    }
}
