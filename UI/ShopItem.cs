using UnityEngine;
using UnityEngine.Events;
using SlayTheTower.Data;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Một ô thẻ trong shop. Lấy <see cref="CardDefinition"/> từ CardView gắn kèm, rồi Mua/Bán qua
    /// <see cref="ShopSystem"/>. Nối nút Mua → <see cref="Buy"/>, nút Bán → <see cref="Sell"/>.
    /// </summary>
    [RequireComponent(typeof(CardView))]
    public class ShopItem : MonoBehaviour
    {
        [Tooltip("CardView của ô này (để trống = lấy trên cùng GameObject).")]
        [SerializeField] private CardView cardView;

        [Header("Sự kiện (cho SFX/feedback)")]
        public UnityEvent onBought;
        public UnityEvent onSold;
        [Tooltip("Mua thiếu vàng / bán khi không có lá.")]
        public UnityEvent onFailed;

        private void Awake()
        {
            if (cardView == null) cardView = GetComponent<CardView>();
        }

        public void Buy()
        {
            var def = cardView != null ? cardView.Definition : null;
            if (def != null && ShopSystem.TryBuy(def))
            {
                cardView.Refresh();
                onBought?.Invoke();
            }
            else onFailed?.Invoke();
        }

        public void Sell()
        {
            var def = cardView != null ? cardView.Definition : null;
            if (def != null && ShopSystem.TrySell(def))
            {
                cardView.Refresh();
                onSold?.Invoke();
            }
            else onFailed?.Invoke();
        }
    }
}
