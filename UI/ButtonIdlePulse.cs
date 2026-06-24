using System.Collections;
using UnityEngine;

namespace PianoGame
{
    /// <summary>
    /// Tự LẶP hiệu ứng nhấn (phóng to + xoay chéo) theo thời gian — kiểu thu hút chú ý.
    /// Mỗi lượt kích hoạt sẽ pulse liên tiếp (mặc định 2 lần), rồi vào thời gian chờ, rồi lặp lại.
    /// Không cần chạm — chạy tự động. Dựa trên ButtonPressEffect.
    /// </summary>
    public class ButtonIdlePulse : MonoBehaviour
    {
        [Header("Hiệu ứng (như ButtonPressEffect)")]
        [Tooltip("Hệ số phóng to mỗi pulse (1.15 = to thêm 15%).")]
        public float pressedScale = 1.15f;
        [Tooltip("Góc xoay chéo mỗi pulse (độ).")]
        public float tiltAngle = 15f;
        [Tooltip("Tốc độ chuyển động (lớn = nảy nhanh).")]
        public float speed = 14f;

        [Header("Lặp theo thời gian")]
        [Tooltip("Số lần pulse LIÊN TIẾP mỗi lượt kích hoạt.")]
        public int pulsesPerActivation = 2;
        [Tooltip("Khoảng nghỉ nhỏ giữa các pulse trong cùng 1 lượt (giây).")]
        public float gapBetweenPulses = 0.05f;
        [Tooltip("Thời gian CHỜ giữa các lượt kích hoạt (giây).")]
        public float interval = 2f;
        [Tooltip("Đổi chiều xoay xen kẽ mỗi pulse (lắc qua lại). TẮT = chỉ xoay 1 góc cố định.")]
        public bool alternateTilt = false;
        [Tooltip("Tự chạy khi bật object.")]
        public bool playOnEnable = true;

        private Vector3 _baseScale;
        private Quaternion _normalRot;
        private Coroutine _loop;

        private void Awake()
        {
            _baseScale = transform.localScale;
            _normalRot = transform.localRotation;
        }

        private void OnEnable()
        {
            if (playOnEnable) StartLoop();
        }

        private void OnDisable()
        {
            StopLoop();
            transform.localScale = _baseScale;
            transform.localRotation = _normalRot;
        }

        /// <summary>Bắt đầu lặp hiệu ứng.</summary>
        public void StartLoop()
        {
            if (_loop == null) _loop = StartCoroutine(Loop());
        }

        /// <summary>Dừng lặp và trả nút về bình thường.</summary>
        public void StopLoop()
        {
            if (_loop != null) { StopCoroutine(_loop); _loop = null; }
            transform.localScale = _baseScale;
            transform.localRotation = _normalRot;
        }

        private IEnumerator Loop()
        {
            while (true)
            {
                int n = Mathf.Max(1, pulsesPerActivation);
                for (int i = 0; i < n; i++)
                {
                    yield return Pulse(i);
                    if (gapBetweenPulses > 0f && i < n - 1)
                        yield return new WaitForSeconds(gapBetweenPulses);
                }
                yield return new WaitForSeconds(Mathf.Max(0f, interval));
            }
        }

        // Một pulse = phóng to + xoay rồi về bình thường.
        private IEnumerator Pulse(int index)
        {
            float dir = (alternateTilt && index % 2 == 1) ? -1f : 1f;
            Quaternion pressedRot = _normalRot * Quaternion.Euler(0f, 0f, tiltAngle * dir);

            yield return MoveTo(_baseScale * pressedScale, pressedRot);
            yield return MoveTo(_baseScale, _normalRot);
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
    }
}
