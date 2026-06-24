using System.Collections.Generic;
using UnityEngine;

namespace SlayTheTower.UI
{
    /// <summary>
    /// HIỂN THỊ TĨNH một TẬP lá cố định (từ CardCollection / List prefab) — hợp cho
    /// màn hình "Bộ sưu tập" / xem-tất-cả-lá / dựng deck.
    ///
    /// ⚠️ KHÔNG dùng cho TAY BÀI TRONG TRẬN. Tay bài trong trận rút từ bộ bài (DeckDefinition)
    /// nên dùng <see cref="HandView"/> (deck-driven: rút/cycling/energy). CardCollection là KHO TỔNG
    /// tất cả lá, không phải bộ bài.
    /// </summary>
    [RequireComponent(typeof(CardHandController))]
    public class CardHandView : MonoBehaviour
    {
        [Header("Nguồn bài (ưu tiên Collection nếu được gán)")]
        [Tooltip("Tùy chọn: gom list prefab trong 1 ScriptableObject.")]
        [SerializeField] private CardCollection collection;
        [Tooltip("Hoặc kéo trực tiếp các prefab lá bài vào đây.")]
        [SerializeField] private List<CardView> cardPrefabs = new();

        [Header("Tham chiếu trong scene")]
        [Tooltip("Nơi chứa các lá bài (RectTransform con của Canvas, full màn hình).")]
        [SerializeField] private RectTransform handRoot;
        [Tooltip("Vùng thả để apply (panel xanh).")]
        [SerializeField] private RectTransform applyZone;

        [Header("Tự dựng")]
        [SerializeField] private bool buildOnStart = true;

        private CardHandController _controller;

        private void Awake() => _controller = GetComponent<CardHandController>();

        private void Start()
        {
            if (buildOnStart) BuildHand();
        }

        /// <summary>Dựng tay bài từ nguồn đã cấu hình.</summary>
        public void BuildHand()
        {
            if (handRoot == null)
            {
                Debug.LogError("[CardHandView] Chưa gán Hand Root.", this);
                return;
            }

            if (applyZone != null) _controller.SetApplyZone(applyZone);

            IReadOnlyList<CardView> source =
                (collection != null && collection.Cards.Count > 0) ? collection.Cards : cardPrefabs;

            foreach (var prefab in source)
            {
                if (prefab == null) continue;

                var cv = Instantiate(prefab, handRoot);
                cv.Refresh();

                var ci = cv.GetComponent<CardInteraction>();
                ci.Setup(_controller, cv.DisplayName);
                _controller.Register(ci);
            }

            _controller.RelayoutHand();
        }
    }
}
