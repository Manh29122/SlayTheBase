using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace PianoGame
{
    /// <summary>
    /// Khi nhấn nút: phóng to lên và xoay chéo nhẹ một góc; thả ra thì trở về bình thường.
    /// SAU KHI hiệu ứng kết thúc (nút về bình thường sau 1 cú click hợp lệ) → gọi <see cref="onPressComplete"/>
    /// (gắn LoadScene / mở ShopMenu... vào đây) và callback từ code (<see cref="PlayThen"/>).
    /// </summary>
    public class ButtonPressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [Tooltip("Hệ số phóng to khi nhấn (1.15 = to thêm 15%).")]
        public float pressedScale = 1.15f;
        [Tooltip("Góc xoay chéo khi nhấn (độ).")]
        public float tiltAngle = 15f;
        [Tooltip("Tốc độ chuyển động (lớn = nảy nhanh).")]
        public float speed = 14f;

        [Tooltip("Chạy SAU KHI hiệu ứng nhấn kết thúc (nút về bình thường). Gắn LoadScene / OpenShop... vào đây.")]
        public UnityEvent onPressComplete;

        private Vector3 _baseScale;
        private Quaternion _normalRot;
        private Quaternion _pressedRot;
        private bool _pressed;
        private bool _actionQueued;   // có 1 click hợp lệ đang chờ effect xong để chạy
        private Action _codeCallback; // callback truyền từ code qua PlayThen
        private Coroutine _routine;
        private Coroutine _oneShot;

        private void Awake()
        {
            _baseScale = transform.localScale;
            _normalRot = transform.localRotation;
            _pressedRot = _normalRot * Quaternion.Euler(0f, 0f, tiltAngle);
        }

        public void OnPointerDown(PointerEventData eventData) => SetPressed(true);
        public void OnPointerUp(PointerEventData eventData) => SetPressed(false);

        // Click hợp lệ = down rồi up cùng trên nút (kéo ra ngoài rồi thả sẽ KHÔNG tính).
        public void OnPointerClick(PointerEventData eventData) => _actionQueued = true;

        /// <summary>Kích hoạt hiệu ứng bằng CODE rồi gọi callback khi hiệu ứng xong (vd thay cho onClick).</summary>
        public void PlayThen(Action onComplete)
        {
            _codeCallback = onComplete;
            if (_oneShot == null) _oneShot = StartCoroutine(OneShotRoutine());
        }

        private void OnDisable()
        {
            // Đảm bảo nút không bị kẹt ở trạng thái phóng to/xoay.
            transform.localScale = _baseScale;
            transform.localRotation = _normalRot;
            _pressed = false;
            _actionQueued = false;
            _routine = null;
            _oneShot = null;
            _codeCallback = null;
        }

        private void SetPressed(bool pressed)
        {
            _pressed = pressed;
            if (_routine == null) _routine = StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            while (true)
            {
                Vector3 targetScale = _pressed ? _baseScale * pressedScale : _baseScale;
                Quaternion targetRot = _pressed ? _pressedRot : _normalRot;

                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * speed);
                transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRot, Time.deltaTime * speed);

                bool reached = (transform.localScale - targetScale).sqrMagnitude < 0.00001f
                               && Quaternion.Angle(transform.localRotation, targetRot) < 0.1f;
                if (reached)
                {
                    transform.localScale = targetScale;
                    transform.localRotation = targetRot;
                    if (!_pressed)
                    {
                        _routine = null;
                        // Hiệu ứng đã kết thúc (về bình thường) → mới chạy thao tác.
                        if (_actionQueued) { _actionQueued = false; FireComplete(); }
                        yield break;
                    }
                    yield return null; // đang giữ -> chờ tới khi thả
                    continue;
                }

                yield return null;
            }
        }

        // Một lượt phóng to -> thu nhỏ -> báo xong (cho PlayThen từ code).
        private IEnumerator OneShotRoutine()
        {
            yield return MoveTo(_baseScale * pressedScale, _pressedRot);
            yield return MoveTo(_baseScale, _normalRot);
            _oneShot = null;
            FireComplete();
        }

        private IEnumerator MoveTo(Vector3 scale, Quaternion rot)
        {
            while ((transform.localScale - scale).sqrMagnitude > 0.00001f
                   || Quaternion.Angle(transform.localRotation, rot) > 0.1f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, scale, Time.deltaTime * speed);
                transform.localRotation = Quaternion.Lerp(transform.localRotation, rot, Time.deltaTime * speed);
                yield return null;
            }
            transform.localScale = scale;
            transform.localRotation = rot;
        }

        private void FireComplete()
        {
            onPressComplete?.Invoke();
            var cb = _codeCallback;
            _codeCallback = null;
            cb?.Invoke();
        }
    }
}
