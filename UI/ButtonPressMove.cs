using UnityEngine;
using UnityEngine.EventSystems;

namespace PianoGame
{
    /// <summary>
    /// Khi NHẤN nút: nút dịch XUỐNG dưới một đoạn; THẢ chuột thì trở về vị trí ban đầu.
    /// Giống cảm giác nhấn nút vật lý. Dùng anchoredPosition (UI).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ButtonPressMove : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Tooltip("Khoảng dịch XUỐNG khi nhấn (pixel).")]
        public float moveDown = 10f;
        [Tooltip("Tốc độ di chuyển (lớn = nhanh). Đặt 0 = nhảy tức thì.")]
        public float speed = 18f;
        [Tooltip("Dùng thời gian KHÔNG bị Time.timeScale ảnh hưởng (UI nên bật).")]
        public bool useUnscaledTime = true;

        private RectTransform _rect;
        private Vector2 _basePos;
        private bool _pressed;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            _basePos = _rect.anchoredPosition;
        }

        public void OnPointerDown(PointerEventData eventData) => _pressed = true;
        public void OnPointerUp(PointerEventData eventData) => _pressed = false;

        private void OnDisable()
        {
            // Đảm bảo nút không kẹt ở vị trí nhấn.
            _pressed = false;
            if (_rect != null) _rect.anchoredPosition = _basePos;
        }

        private void Update()
        {
            Vector2 target = _pressed ? _basePos + Vector2.down * moveDown : _basePos;

            if (speed <= 0f) { _rect.anchoredPosition = target; return; }

            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _rect.anchoredPosition = Vector2.Lerp(_rect.anchoredPosition, target, dt * speed);
        }
    }
}
