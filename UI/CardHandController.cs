using System.Collections.Generic;
using UnityEngine;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Điều phối cả bộ bài trên tay: bố cục dạng QUẠT (fan) kiểu Slay the Spire
    /// (đè mép, nghiêng chéo ra rìa, lá giữa thẳng), chọn/phóng to 1 lá và đẩy các lá khác ra,
    /// kiểm tra điểm thả vào vùng apply.
    /// </summary>
    public class CardHandController : MonoBehaviour
    {
        [Header("Bố cục quạt (fan)")]
        [Tooltip("Khoảng cách tâm-tâm giữa 2 lá. Nhỏ hơn bề rộng lá để các mép đè lên nhau.")]
        [SerializeField] private float spacing = 120f;
        [Tooltip("Độ nghiêng (độ) tăng dần khi ra rìa. Lá giữa = 0.")]
        [SerializeField] private float anglePerCard = 7f;
        [Tooltip("Độ võng xuống ở rìa (theo bình phương khoảng cách tới giữa).")]
        [SerializeField] private float arcDrop = 16f;
        [Tooltip("Cao độ của hàng bài.")]
        [SerializeField] private float baseY = -360f;

        [Header("Khi chọn / phóng to")]
        [SerializeField] private float selectedScale = 1.7f;
        [Tooltip("Nâng lá đang chọn lên bao nhiêu pixel.")]
        [SerializeField] private float selectedLift = 200f;
        [Tooltip("Đẩy các lá xung quanh ra hai bên để tạo khoảng trống.")]
        [SerializeField] private float pushAmount = 110f;

        [Header("Apply")]
        [SerializeField] private RectTransform applyZone;

        public float SelectedScale => selectedScale;
        /// <summary>(lá, vị trí màn hình khi thả, camera) — bắn khi kéo thả vào vùng apply.</summary>
        public System.Action<CardInteraction, Vector2, Camera> OnCardApplied;

        private readonly List<CardInteraction> _cards = new();
        private CardInteraction _selected;
        private CardInteraction _dragged;

        public void SetApplyZone(RectTransform zone) => applyZone = zone;

        public void Register(CardInteraction card)
        {
            if (card != null && !_cards.Contains(card))
                _cards.Add(card);
        }

        public void Unregister(CardInteraction card)
        {
            _cards.Remove(card);
            if (_selected == card) _selected = null;
            if (_dragged == card) _dragged = null;
        }

        public void SelectCard(CardInteraction card)
        {
            // Toggle: chạm lá đang chọn lần nữa -> bỏ chọn (thu nhỏ).
            _selected = (_selected == card) ? null : card;
            RelayoutHand();
        }

        public void DeselectAll()
        {
            _selected = null;
            RelayoutHand();
        }

        public void OnCardBeginDrag(CardInteraction card)
        {
            if (_selected == card) _selected = null;
            _dragged = card;
            RelayoutHand(); // các lá còn lại khép về quạt chuẩn
        }

        public void OnCardReturned(CardInteraction card)
        {
            _dragged = null;
            RelayoutHand(); // lá trượt về đúng khe trong quạt
        }

        public bool IsInApplyZone(Vector2 screenPos, Camera cam)
            => applyZone != null &&
               RectTransformUtility.RectangleContainsScreenPoint(applyZone, screenPos, cam);

        public void ApplyCard(CardInteraction card, Vector2 screenPos, Camera cam)
        {
            OnCardApplied?.Invoke(card, screenPos, cam);
            _dragged = null;
            if (_selected == card) _selected = null;
            RelayoutHand();
        }

        /// <summary>Tính lại vị trí/góc/scale cho toàn bộ lá theo bố cục quạt.</summary>
        public void RelayoutHand()
        {
            int n = _cards.Count;
            if (n == 0) return;

            float center = (n - 1) * 0.5f;
            int selIndex = _selected != null ? _cards.IndexOf(_selected) : -1;

            for (int i = 0; i < n; i++)
            {
                var card = _cards[i];
                float d = i - center; // <0 bên trái, >0 bên phải, 0 ở giữa

                float x = d * spacing;
                float y = baseY - arcDrop * d * d;   // võng xuống ở rìa
                float rot = -d * anglePerCard;        // nghiêng chéo, giữa = 0
                float scale = 1f;

                if (selIndex >= 0)
                {
                    if (i == selIndex)
                    {
                        rot = 0f;           // lá đang xem: thẳng đứng
                        y += selectedLift;  // nâng lên
                        scale = selectedScale;
                    }
                    else
                    {
                        // đẩy lá bên trái sang trái, bên phải sang phải -> tạo khoảng trống
                        x += i < selIndex ? -pushAmount : pushAmount;
                    }
                }

                card.transform.SetSiblingIndex(i); // lá phải đè lên lá trái
                card.SetTarget(new Vector2(x, y), rot, scale);

                // Bật viền cho lá đang chọn, tắt cho các lá khác.
                if (card.TryGetComponent<CardSelectionBorder>(out var border))
                    border.SetSelected(i == selIndex);
            }

            // Lá đang kéo/đang xem luôn nổi trên cùng.
            if (_dragged != null) _dragged.transform.SetAsLastSibling();
            else if (_selected != null) _selected.transform.SetAsLastSibling();
        }
    }
}
