using System.Collections;
using UnityEngine;

namespace PianoGame
{
    /// <summary>
    /// Pulse (zoom punch) kích thước camera: <c>orthographicSize</c> phình ra rồi mượt về gốc.
    /// Gọi <see cref="Pulse()"/> bằng code/UnityEvent để kích hoạt (vd khi trúng đòn, triệu hồi...).
    /// </summary>
    public class PulseCameraSize : MonoBehaviour
    {
        [Tooltip("Camera cần pulse (để trống = Camera trên object này, hoặc Camera.main).")]
        public Camera targetCamera;
        [Tooltip("Biên độ pulse. Dương = zoom XA (size to ra), âm = zoom GẦN (size nhỏ lại). 0.1 = ±10%.")]
        public float amount = 0.1f;
        [Tooltip("Thời lượng 1 nhịp pulse (giây).")]
        public float duration = 0.3f;
        [Tooltip("Dùng thời gian KHÔNG bị Time.timeScale ảnh hưởng.")]
        public bool useUnscaledTime = true;

        private float _baseSize;
        private Coroutine _routine;

        private void Reset() => targetCamera = GetComponent<Camera>();

        private void Awake()
        {
            if (targetCamera == null) targetCamera = GetComponent<Camera>();
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera != null) _baseSize = targetCamera.orthographicSize;
        }

        /// <summary>Pulse camera size bằng biên độ/thời lượng mặc định (Inspector).</summary>
        public void Pulse() => Pulse(amount, duration);

        /// <summary>Pulse camera size với biên độ + thời lượng tuỳ chỉnh.</summary>
        public void Pulse(float pulseAmount, float pulseDuration)
        {
            if (targetCamera == null) return;
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(PulseRoutine(pulseAmount, Mathf.Max(0.01f, pulseDuration)));
        }

        /// <summary>Dừng pulse và trả size về gốc.</summary>
        public void StopPulse()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = null;
            if (targetCamera != null) targetCamera.orthographicSize = _baseSize;
        }

        /// <summary>Cập nhật size gốc (vd khi đổi zoom runtime) để pulse tính lại từ đây.</summary>
        public void SetBaseSize(float size)
        {
            _baseSize = size;
            if (targetCamera != null) targetCamera.orthographicSize = size;
        }

        private IEnumerator PulseRoutine(float amt, float dur)
        {
            float t = 0f;
            while (t < dur)
            {
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                // Envelope 0 -> 1 -> 0 mượt (sin nửa chu kỳ), đỉnh ở giữa.
                float k = Mathf.Sin(Mathf.Clamp01(t / dur) * Mathf.PI);
                targetCamera.orthographicSize = _baseSize * (1f + amt * k);
                yield return null;
            }
            targetCamera.orthographicSize = _baseSize;
            _routine = null;
        }
    }
}
