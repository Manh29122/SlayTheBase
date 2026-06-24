using System.Collections.Generic;
using UnityEngine;
using SlayTheTower.Data;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Màn TẠO DECK chia 2 vùng: TRÁI = các lá đang sở hữu (mỗi bản 1 prefab — có 3 lá Chiến binh
    /// thì spawn 3 lá), PHẢI = deck. Kéo lá từ trái sang phải để thêm vào deck (lá rời khỏi vùng trái).
    /// Kéo ngược lại phải→trái để bỏ khỏi deck. (Việc nhận lá do <see cref="DeckDropZone"/> trên mỗi vùng.)
    /// </summary>
    public class DeckBuilder : MonoBehaviour
    {
        [Header("2 vùng (mỗi cái nên có DeckDropZone + LayoutGroup + Image raycast)")]
        [Tooltip("Vùng TRÁI: lá đang sở hữu.")]
        [SerializeField] private Transform collectionContainer;
        [Tooltip("Vùng PHẢI: deck.")]
        [SerializeField] private Transform deckContainer;

        [Header("Thẻ")]
        [Tooltip("Prefab thẻ (có CardView). Dùng chung mọi lá; SetDefinition lúc spawn.")]
        [SerializeField] private GameObject cardPrefab;
        [Tooltip("Thẻ designer (asset) để map id -> định nghĩa.")]
        [SerializeField] private List<CardDefinition> availableCards = new();
        [Tooltip("Nạp luôn thẻ NGƯỜI CHƠI tự tạo (PlayerCardStorage) không.")]
        [SerializeField] private bool includePlayerCards = true;

        private readonly Dictionary<string, CardDefinition> _byId = new();

        private void Start()
        {
            BuildIndex();
            Populate();
            LoadSavedDeck();
        }

        private void BuildIndex()
        {
            _byId.Clear();
            foreach (var c in availableCards)
                if (c != null && !string.IsNullOrEmpty(c.id)) _byId[c.id] = c;

            if (includePlayerCards)
                foreach (var d in PlayerCardStorage.LoadAll())
                {
                    var def = RuntimeCardFactory.Build(d, cardPrefab, RuntimeUnitFactory.BuildById);
                    if (def != null && !string.IsNullOrEmpty(def.id)) _byId[def.id] = def;
                }
        }

        /// <summary>Spawn các lá ĐANG SỞ HỮU vào vùng trái — mỗi bản 1 prefab.</summary>
        public void Populate()
        {
            Clear(collectionContainer);
            Clear(deckContainer);

            foreach (var kv in CardInventory.GetAll())
            {
                if (!_byId.TryGetValue(kv.Key, out var def)) continue;
                for (int i = 0; i < kv.Value; i++)
                    SpawnCard(def, collectionContainer);
            }
        }

        private void SpawnCard(CardDefinition def, Transform parent)
        {
            var go = Instantiate(cardPrefab, parent);
            var view = go.GetComponent<CardView>();
            if (view != null) view.SetDefinition(def);

            // Tắt tương tác kiểu tay-bài để không xung đột với kéo-thả deck.
            var ci = go.GetComponent<CardInteraction>();
            if (ci != null) ci.enabled = false;

            if (go.GetComponent<CanvasGroup>() == null) go.AddComponent<CanvasGroup>();
            if (go.GetComponent<DeckCardDrag>() == null) go.AddComponent<DeckCardDrag>();
        }

        /// <summary>Đưa các lá trong deck đã lưu sang vùng phải (phần còn lại nằm bên trái).</summary>
        public void LoadSavedDeck()
        {
            foreach (var id in PlayerDeckStorage.Load())
            {
                var card = FindInContainer(collectionContainer, id);
                if (card != null) card.SetParent(deckContainer, false);
            }
        }

        /// <summary>Lưu deck = các lá đang ở vùng PHẢI. Nối vào nút "Lưu Deck".</summary>
        public void SaveDeck()
        {
            var ids = new List<string>();
            foreach (Transform child in deckContainer)
            {
                var v = child.GetComponent<CardView>();
                if (v != null && v.Definition != null) ids.Add(v.Definition.id);
            }
            PlayerDeckStorage.Save(ids);
            Debug.Log($"[DeckBuilder] Đã lưu deck {ids.Count} lá.");
        }

        private static Transform FindInContainer(Transform container, string id)
        {
            if (container == null) return null;
            foreach (Transform child in container)
            {
                var v = child.GetComponent<CardView>();
                if (v != null && v.Definition != null && v.Definition.id == id) return child;
            }
            return null;
        }

        private static void Clear(Transform container)
        {
            if (container == null) return;
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }
    }
}
