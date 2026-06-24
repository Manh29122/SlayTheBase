using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SlayTheTower.Data;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Panel xem bộ sưu tập lá bài. Open/Close panel; nạp các thẻ vào Container (có GridLayoutGroup tự xếp).
    /// Nên đặt script trên 1 object LUÔN BẬT (vd Canvas) và gán Panel; hoặc đặt ngay trên Panel cũng được.
    /// </summary>
    public class CollectionPanel : MonoBehaviour
    {
        [Header("Tham chiếu")]
        [Tooltip("Panel để bật/tắt. Để trống = chính GameObject này.")]
        [SerializeField] private GameObject panel;
        [Tooltip("Container có GridLayoutGroup — nơi chứa các thẻ.")]
        [SerializeField] private Transform container;
        [Tooltip("Nút đóng (tùy chọn — sẽ tự nối vào Close()).")]
        [SerializeField] private Button closeButton;

        [Header("Nguồn thẻ")]
        [Tooltip("Bộ sưu tập mặc định (list prefab CardView). Tự nạp khi Open nếu bật Load On Open.")]
        [SerializeField] private CardCollection collection;
        [Tooltip("Prefab thẻ dùng khi nạp theo danh sách CardDefinition.")]
        [SerializeField] private CardView cardTemplate;
        [SerializeField] private bool loadOnOpen = true;

        private GameObject Panel => panel != null ? panel : gameObject;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        // ---------- Open / Close ----------
        public void Open()
        {
            Panel.SetActive(true);
            if (loadOnOpen && collection != null) LoadFromCollection(collection);
        }

        public void Close() => Panel.SetActive(false);

        public void Toggle()
        {
            if (Panel.activeSelf) Close();
            else Open();
        }

        // ---------- Nạp thẻ vào Container ----------
        /// <summary>Nạp từ CardCollection (instantiate từng prefab CardView vào Container).</summary>
        public void LoadFromCollection(CardCollection source)
        {
            ClearContainer();
            if (source == null || container == null) return;

            foreach (var prefab in source.Cards)
            {
                if (prefab == null) continue;
                var cv = Instantiate(prefab, container);
                cv.Refresh();
            }
        }

        /// <summary>Nạp từ danh sách CardDefinition (dùng chung 1 prefab cardTemplate).</summary>
        public void LoadCards(IReadOnlyList<CardDefinition> cards)
        {
            ClearContainer();
            if (cards == null || container == null) return;
            if (cardTemplate == null)
            {
                Debug.LogWarning("[CollectionPanel] Chưa gán Card Template để nạp theo CardDefinition.", this);
                return;
            }

            foreach (var def in cards)
            {
                if (def == null) continue;
                var cv = Instantiate(cardTemplate, container);
                cv.SetDefinition(def);
            }
        }

        /// <summary>Xoá hết thẻ đang có trong Container.</summary>
        public void ClearContainer()
        {
            if (container == null) return;
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }
    }
}
