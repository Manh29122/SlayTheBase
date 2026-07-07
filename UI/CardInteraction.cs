using UnityEngine;
using UnityEngine.EventSystems;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Tương tác cho 1 lá bài trên Canvas — GỘP 2 việc:
    /// 1) Tay bài (khi có <see cref="CardHandController"/>): nội suy về vị trí/góc/scale quạt + kéo để đánh bài.
    /// 2) Hiệu ứng hover kiểu Balatro: con trỏ chạm → lá NGHIÊNG 3D (góc gần con trỏ nâng, đối diện hạ),
    ///    lò xo tạo RUNG/DAO ĐỘNG; giữ thì kéo theo con trỏ; nhả về chỗ cũ (nếu KHÔNG thuộc tay bài).
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class CardInteraction : MonoBehaviour,
        IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        [Tooltip("Tốc độ nội suy về vị trí/scale/góc mục tiêu.")]
        [SerializeField] private float animSpeed = 14f;

        [Header("Hiệu ứng hover kiểu Balatro")]
        [Tooltip("Bật nghiêng/rung khi con trỏ chạm lá.")]
        [SerializeField] private bool enableHoverTilt = true;
        [Tooltip("Góc nghiêng tối đa khi con trỏ ở góc (độ).")]
        [SerializeField] private float maxTilt = 16f;
        [Tooltip("Bật nếu thấy hướng nâng/hạ bị ngược.")]
        [SerializeField] private bool invertTilt = false;
        [Tooltip("Độ cứng lò xo (lớn = bám nhanh).")]
        [SerializeField] private float tiltStiffness = 120f;
        [Tooltip("Giảm chấn (nhỏ = rung/dao động lâu hơn).")]
        [SerializeField] private float tiltDamping = 14f;
        [Tooltip("Nghiêng thêm theo vận tốc kéo (cảm giác dao động khi vẩy).")]
        [SerializeField] private float velocityTilt = 0.06f;
        [Tooltip("Phóng to khi hover/giữ.")]
        [SerializeField] private float hoverScale = 1.08f;

        [Header("Kéo có độ trễ + Bóng")]
        [Tooltip("Tốc độ lá ĐUỔI theo con trỏ khi kéo (NHỎ = trễ/lả lướt hơn; chuột dừng thì lá từ từ bắt kịp).")]
        [SerializeField] private float dragFollowSpeed = 12f;
        [Tooltip("(Tùy chọn) Bóng của lá — RectTransform con trong prefab. Sẽ di chuyển TRỄ so với lá.")]
        [SerializeField] private RectTransform shadow;
        [Tooltip("Bóng bị kéo lại bao nhiêu khi lá di chuyển (lớn = trễ rõ).")]
        [SerializeField] private float shadowLag = 0.12f;
        [Tooltip("Tốc độ bóng trở về vị trí gốc khi lá đứng yên (lớn = bắt kịp nhanh).")]
        [SerializeField] private float shadowCatchUp = 8f;

        public string CardName { get; private set; }

        private RectTransform _rect;
        private CanvasGroup _group;
        private Canvas _canvas;
        private CardHandController _hand;

        private Vector2 _targetPos;
        private float _targetRot;
        private float _targetScale = 1f;
        private bool _dragging;

        // Base (cho chế độ standalone — không thuộc tay bài).
        private Vector2 _basePos;
        private float _baseZ;
        private float _baseScaleMag = 1f;

        // Trạng thái hover / lò xo nghiêng.
        private bool _hovering;
        private Vector2 _pointerScreen;
        private Vector2 _pointerLocal;   // -0.5..0.5 trong rect
        private Vector2 _dragVelocity;
        private float _tiltX, _tiltXVel;
        private float _tiltY, _tiltYVel;
        private float _curScale = 1f;
        private float _curZ;

        // Kéo trễ + bóng.
        private Vector2 _dragTargetPos;
        private Vector2 _prevPos;
        private Vector2 _shadowBase;
        private Vector2 _shadowOffset;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            _group = GetComponent<CanvasGroup>();
            _canvas = GetComponentInParent<Canvas>();
            CaptureBase();

            _prevPos = _rect.anchoredPosition;
            if (shadow != null) { _shadowBase = shadow.anchoredPosition; _shadowOffset = _shadowBase; }
        }

        /// <summary>Ghi lại vị trí/góc/scale gốc (dùng cho chế độ standalone).</summary>
        public void CaptureBase()
        {
            _basePos = _rect.anchoredPosition;
            _baseZ = _rect.localEulerAngles.z;
            _baseScaleMag = _rect.localScale.x;
            _curScale = _baseScaleMag;
            _curZ = _baseZ;
            _targetPos = _basePos;
        }

        public void Setup(CardHandController hand, string cardName)
        {
            if (_rect == null) _rect = (RectTransform)transform;
            if (_group == null) _group = GetComponent<CanvasGroup>();
            _hand = hand;
            CardName = cardName;
            _targetPos = _rect.anchoredPosition;
        }

        /// <summary>Controller gọi để đặt trạng thái nghỉ cho lá bài (tay bài).</summary>
        public void SetTarget(Vector2 pos, float rotationZ, float scale)
        {
            _targetPos = pos;
            _targetRot = rotationZ;
            _targetScale = scale;
        }

        private void Update()
        {
            if (_rect == null) return;

            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.033f);
            float t = animSpeed * dt;

            UpdatePointerLocal();

            // ----- Lò xo nghiêng (rung + dao động) -----
            bool active = enableHoverTilt && (_hovering || _dragging);
            float tgtX = 0f, tgtY = 0f;
            if (active)
            {
                float s = invertTilt ? -1f : 1f;
                tgtX = -_pointerLocal.y * 2f * maxTilt * s; // con trỏ lên trên -> mép trên nâng
                tgtY =  _pointerLocal.x * 2f * maxTilt * s; // con trỏ sang phải -> mép phải nâng
                if (_dragging)
                {
                    tgtY += Mathf.Clamp(_dragVelocity.x * velocityTilt, -maxTilt, maxTilt);
                    tgtX += Mathf.Clamp(-_dragVelocity.y * velocityTilt, -maxTilt, maxTilt);
                }
            }
            Spring(ref _tiltX, ref _tiltXVel, tgtX, dt);
            Spring(ref _tiltY, ref _tiltYVel, tgtY, dt);
            _dragVelocity = Vector2.Lerp(_dragVelocity, Vector2.zero, dt * 10f);

            // ----- Nền: vị trí / z-rot / scale -----
            if (_dragging)
            {
                // ĐUỔI theo con trỏ có ĐỘ TRỄ (không bám sát); chuột dừng -> lá từ từ bắt kịp.
                _rect.anchoredPosition = Vector2.Lerp(_rect.anchoredPosition, _dragTargetPos, dt * dragFollowSpeed);
                float dragScale = _hand != null ? _hand.SelectedScale : _baseScaleMag;
                _curScale = Mathf.Lerp(_curScale, dragScale, t);
                _curZ = Mathf.LerpAngle(_curZ, 0f, t);
            }
            else
            {
                Vector2 restPos = _hand != null ? _targetPos : _basePos;
                float restZ = _hand != null ? _targetRot : _baseZ;
                float restScale = _hand != null ? _targetScale : _baseScaleMag;

                _rect.anchoredPosition = Vector2.Lerp(_rect.anchoredPosition, restPos, t);
                _curScale = Mathf.Lerp(_curScale, restScale, t);
                _curZ = Mathf.LerpAngle(_curZ, restZ, t);
            }

            float scaleMul = active ? hoverScale : 1f;
            _rect.localRotation = Quaternion.Euler(_tiltX, _tiltY, _curZ);
            _rect.localScale = Vector3.one * (_curScale * scaleMul);

            UpdateShadow(dt);
        }

        // Bóng bị "kéo lại" khi lá di chuyển, rồi từ từ trở về offset gốc -> trông như trễ so với lá.
        private void UpdateShadow(float dt)
        {
            if (shadow == null) return;
            Vector2 pos = _rect.anchoredPosition;
            Vector2 delta = pos - _prevPos;
            _shadowOffset -= delta * shadowLag;
            _shadowOffset = Vector2.Lerp(_shadowOffset, _shadowBase, dt * shadowCatchUp);
            shadow.anchoredPosition = _shadowOffset;
            _prevPos = pos;
        }

        // ----- Hover -----
        public void OnPointerEnter(PointerEventData e) { _hovering = true; _pointerScreen = e.position; }
        public void OnPointerExit(PointerEventData e) { _hovering = false; }
        public void OnPointerMove(PointerEventData e) { _pointerScreen = e.position; }

        // ----- Chạm = chọn (chỉ khi thuộc tay bài) -----
        public void OnPointerClick(PointerEventData e)
        {
            if (_hand == null || _dragging) return;
            _hand.SelectCard(this);
        }

        // ----- Nhấn-giữ-kéo (cả tay bài lẫn standalone) -----
        public void OnBeginDrag(PointerEventData e)
        {
            _dragging = true;
            _pointerScreen = e.position;
            _dragTargetPos = _rect.anchoredPosition; // bắt đầu từ chỗ hiện tại, khỏi nhảy
            if (_group != null) _group.blocksRaycasts = false;
            if (_hand != null) _hand.OnCardBeginDrag(this);
        }

        public void OnDrag(PointerEventData e)
        {
            _pointerScreen = e.position;
            _dragVelocity = e.delta;
            // Chỉ LƯU vị trí đích; Update sẽ lerp tới có độ trễ.
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    (RectTransform)_rect.parent, e.position, EventCam(e.pressEventCamera), out var local))
                _dragTargetPos = local;
        }

        public void OnEndDrag(PointerEventData e)
        {
            _dragging = false;
            _dragVelocity = Vector2.zero;
            if (_group != null) _group.blocksRaycasts = true;

            if (_hand != null)
            {
                if (_hand.IsInApplyZone(e.position, e.pressEventCamera))
                    _hand.ApplyCard(this, e.position, e.pressEventCamera);
                else
                    _hand.OnCardReturned(this);
            }
            // Standalone: Update tự lerp về _basePos.
        }

        // ----- Helpers hover -----
        private void UpdatePointerLocal()
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, _pointerScreen, EventCam(null), out var local))
            {
                var r = _rect.rect;
                float nx = r.width  > 0f ? (local.x - r.center.x) / r.width  : 0f;
                float ny = r.height > 0f ? (local.y - r.center.y) / r.height : 0f;
                _pointerLocal = new Vector2(Mathf.Clamp(nx, -0.5f, 0.5f), Mathf.Clamp(ny, -0.5f, 0.5f));
            }
        }

        private Camera EventCam(Camera fromEvent)
        {
            if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
            return fromEvent != null ? fromEvent : (_canvas != null ? _canvas.worldCamera : null);
        }

        private void Spring(ref float value, ref float vel, float target, float dt)
        {
            float force = (target - value) * tiltStiffness - vel * tiltDamping;
            vel += force * dt;
            value += vel * dt;
        }
    }
}
