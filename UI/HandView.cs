using System.Collections.Generic;
using UnityEngine;
using SlayTheTower.Data;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Tay bài DECK-DRIVEN: rút bài từ <see cref="BattleDeck"/>, hiển thị theo quạt
    /// (qua <see cref="CardHandController"/>), đánh thẻ thì trừ energy + chạy hiệu ứng
    /// + trả lá về cuối deck + rút bù. Cung cấp các thao tác deck/tay cho buff (rút thêm,
    /// recycle, đổi giới hạn tay...).
    /// </summary>
    [RequireComponent(typeof(CardHandController))]
    public class HandView : MonoBehaviour
    {
        [Header("Hệ thống")]
        [SerializeField] private EnergySystem energy;
        [SerializeField] private BattleDeck deck;
        [SerializeField] private BattleContext battleContext;

        [Header("Tham chiếu UI")]
        [SerializeField] private RectTransform handRoot;
        [SerializeField] private RectTransform applyZone;
        [Tooltip("Prefab thẻ mặc định, dùng khi CardDefinition không có cardPrefab riêng.")]
        [SerializeField] private GameObject fallbackCardPrefab;

        [Header("Nguồn bài")]
        [Tooltip("Bộ bài khởi đầu. Để trống nếu deck được nạp từ nơi khác trước khi BuildHand().")]
        [SerializeField] private DeckDefinition deckDefinition;
        [SerializeField] private bool buildOnStart = true;

        private CardHandController _controller;
        private readonly List<CardView> _hand = new();
        private bool _built;

        public IReadOnlyList<CardView> Hand => _hand;

        private void Awake() => _controller = GetComponent<CardHandController>();

        private void Start()
        {
            if (buildOnStart) BuildHand();
        }

        /// <summary>Gán nhanh tham chiếu bằng code (dùng cho demo/bootstrap).</summary>
        public void Configure(EnergySystem energySystem, BattleDeck battleDeck, BattleContext context,
            RectTransform hand, RectTransform apply, GameObject fallbackPrefab)
        {
            energy = energySystem;
            deck = battleDeck;
            battleContext = context;
            handRoot = hand;
            applyZone = apply;
            fallbackCardPrefab = fallbackPrefab;
        }

        private void OnDestroy()
        {
            if (_controller != null) _controller.OnCardApplied -= HandleCardApplied;
        }

        public void BuildHand()
        {
            if (_built) return;
            if (handRoot == null) { Debug.LogError("[HandView] Chưa gán Hand Root.", this); return; }

            // Tự lo BattleDeck nếu chưa gán (đỡ phải thêm component tay).
            if (deck == null) deck = GetComponent<BattleDeck>();
            if (deck == null) deck = gameObject.AddComponent<BattleDeck>();
            _built = true;

            if (applyZone != null) _controller.SetApplyZone(applyZone);
            _controller.OnCardApplied += HandleCardApplied;

            if (deckDefinition != null)
                deck.Initialize(deckDefinition.BuildCardList(), deckDefinition.startingHandLimit);
            else
                Debug.LogWarning("[HandView] Chưa gán Deck Definition — tay bài sẽ rỗng. " +
                    "Tạo Deck ở Slay The Tower/Level Editor → tab Deck rồi gán vào đây.", this);

            Refill();
        }

        // ----- Đánh thẻ -----
        private void HandleCardApplied(CardInteraction ci, Vector2 screenPos, Camera cam)
        {
            if (battleContext != null) battleContext.SetCastFromScreen(screenPos, cam);
            var cv = ci != null ? ci.GetComponent<CardView>() : null;
            TryPlay(cv);
        }

        public bool TryPlay(CardView cv)
        {
            if (cv == null) return false;

            bool free = deck.HasFreeCard;
            float cost = free ? 0f : deck.EffectiveCost(cv.Definition);
            if (energy != null && !energy.TrySpend(cost))
                return false; // không đủ energy -> lá tự về chỗ cũ (controller đã relayout)

            if (free) deck.ConsumeFreeCard();

            var def = cv.Definition;
            if (def != null && battleContext != null)
            {
                if (def.cardType == CardType.Buff)
                    CardEffectResolver.ExecuteAll(def.effects, battleContext);
                else
                    battleContext.SummonFromCard(def);
            }

            RemoveCard(cv, returnToDeck: true);
            Refill();
            return true;
        }

        // ----- Rút / bù bài -----
        public void Refill()
        {
            int safety = 100;
            while (_hand.Count < deck.HandLimit && safety-- > 0)
            {
                var def = deck.DrawOne();
                if (def == null) break;
                SpawnCard(def);
            }
            _controller.RelayoutHand();
            RefreshCostLabels();
        }

        public void DrawCards(int n)
        {
            for (int i = 0; i < n; i++)
            {
                var def = deck.DrawOne();
                if (def == null) break;
                SpawnCard(def);
            }
            _controller.RelayoutHand();
            RefreshCostLabels();
        }

        /// <summary>Cập nhật nhãn cost của mọi lá trên tay theo chi phí hiệu lực (giảm giá/nâng cấp).</summary>
        public void RefreshCostLabels()
        {
            foreach (var cv in _hand)
            {
                if (cv == null || cv.Definition == null) continue;
                cv.SetDisplayedCost(Mathf.CeilToInt(deck.EffectiveCost(cv.Definition)));
            }
        }

        /// <summary>Nâng cấp N lá ngẫu nhiên đang trên tay (cả run).</summary>
        public void UpgradeRandomCards(int n)
        {
            for (int i = 0; i < n; i++)
            {
                if (_hand.Count == 0) break;
                var cv = _hand[Random.Range(0, _hand.Count)];
                if (cv != null && cv.Definition != null) deck.Upgrade(cv.Definition, 1);
            }
            RefreshCostLabels();
        }

        /// <summary>Trả N lá ngẫu nhiên từ tay về deck rồi rút M lá.</summary>
        public void ReturnRandomThenDraw(int returnCount, int drawCount)
        {
            for (int i = 0; i < returnCount && _hand.Count > 0; i++)
            {
                var cv = _hand[Random.Range(0, _hand.Count)];
                RemoveCard(cv, returnToDeck: true);
            }
            DrawCards(drawCount);
        }

        /// <summary>Úp cả tay xuống đáy deck rồi rút N lá mới.</summary>
        public void RecycleHandThenDraw(int drawCount)
        {
            foreach (var cv in _hand.ToArray())
                RemoveCard(cv, returnToDeck: true);
            DrawCards(drawCount);
        }

        // ----- Nội bộ -----
        private CardView SpawnCard(CardDefinition def)
        {
            var prefab = (def != null && def.cardPrefab != null) ? def.cardPrefab : fallbackCardPrefab;
            if (prefab == null) { Debug.LogError("[HandView] Không có prefab cho thẻ.", this); return null; }

            var go = Instantiate(prefab, handRoot);
            go.SetActive(true); // phòng trường hợp prefab/template ở trạng thái inactive
            var cv = go.GetComponent<CardView>();
            if (cv == null) { Debug.LogError("[HandView] Prefab thẻ thiếu CardView.", go); Destroy(go); return null; }

            cv.SetDefinition(def);
            var ci = cv.GetComponent<CardInteraction>();
            ci.Setup(_controller, cv.DisplayName);
            _controller.Register(ci);
            _hand.Add(cv);
            return cv;
        }

        private void RemoveCard(CardView cv, bool returnToDeck)
        {
            if (cv == null) return;
            _hand.Remove(cv);
            _controller.Unregister(cv.GetComponent<CardInteraction>());
            if (returnToDeck) deck.ReturnToBottom(cv.Definition);
            Destroy(cv.gameObject);
        }
    }
}
