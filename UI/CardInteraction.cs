using UnityEngine;
using UnityEngine.EventSystems;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Tương tác cho 1 lá bài trên Canvas. Trạng thái nghỉ (vị trí/góc/scale trong quạt)
    /// do <see cref="CardHandController"/> tính và đẩy xuống qua <see cref="SetTarget"/>.
    /// Lá bài chỉ việc nội suy mượt về mục tiêu, và xử lý kéo-thả.
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class CardInteraction : MonoBehaviour,
        IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Tooltip("Tốc độ nội suy về vị trí/scale/góc mục tiêu.")]
        [SerializeField] private float animSpeed = 14f;

        public string CardName { get; private set; }

        private RectTransform _rect;
        private CanvasGroup _group;
        private CardHandController _hand;

        private Vector2 _targetPos;
        private float _targetRot;
        private float _targetScale = 1f;
        private bool _dragging;

        public void Setup(CardHandController hand, string cardName)
        {
            _rect = (RectTransform)transform;
            _group = GetComponent<CanvasGroup>();
            _hand = hand;
            CardName = cardName;
            _targetPos = _rect.anchoredPosition;
        }

        /// <summary>Controller gọi để đặt trạng thái nghỉ cho lá bài.</summary>
        public void SetTarget(Vector2 pos, float rotationZ, float scale)
        {
            _targetPos = pos;
            _targetRot = rotationZ;
            _targetScale = scale;
        }

        private void Update()
        {
            if (_rect == null) return; // chưa Setup (vd dùng CardView làm preview tĩnh)

            float t = animSpeed * Time.unscaledDeltaTime;

            if (_dragging)
            {
                // Khi kéo: thẳng góc + phóng to; vị trí do OnDrag điều khiển.
                _rect.localScale = Vector3.Lerp(_rect.localScale, Vector3.one * _hand.SelectedScale, t);
                _rect.localRotation = Quaternion.Euler(0f, 0f,
                    Mathf.LerpAngle(_rect.localEulerAngles.z, 0f, t));
                return;
            }

            _rect.anchoredPosition = Vector2.Lerp(_rect.anchoredPosition, _targetPos, t);
            _rect.localScale = Vector3.Lerp(_rect.localScale, Vector3.one * _targetScale, t);
            _rect.localRotation = Quaternion.Euler(0f, 0f,
                Mathf.LerpAngle(_rect.localEulerAngles.z, _targetRot, t));
        }

        // ----- Chạm = chọn/phóng to -----
        public void OnPointerClick(PointerEventData e)
        {
            if (_hand == null) return; // chưa thuộc tay bài (vd hiển thị trong Collection) -> vô hại
            if (_dragging) return;
            _hand.SelectCard(this);
        }

        // ----- Nhấn-giữ-kéo -----
        public void OnBeginDrag(PointerEventData e)
        {
            if (_hand == null) return;
            _dragging = true;
            _group.blocksRaycasts = false;
            _hand.OnCardBeginDrag(this);
        }

        public void OnDrag(PointerEventData e)
        {
            if (_hand == null) return;
            var parent = (RectTransform)_rect.parent;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parent, e.position, e.pressEventCamera, out var local))
                _rect.anchoredPosition = local;
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (_hand == null) return;
            _dragging = false;
            _group.blocksRaycasts = true;

            if (_hand.IsInApplyZone(e.position, e.pressEventCamera))
                _hand.ApplyCard(this, e.position, e.pressEventCamera);
            else
                _hand.OnCardReturned(this);
        }
    }
}
