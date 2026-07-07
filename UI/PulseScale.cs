using System.Collections;
using UnityEngine;

namespace PianoGame
{
    /// <summary>
    /// Pulse (phình to → về gốc) scale theo nhịp: mỗi lần pulse kéo dài <see cref="pulseDuration"/>,
    /// rồi NGHỈ <see cref="gapBetween"/> giây trước khi pulse lần tiếp theo. Lặp liên tục.
    /// Tự chạy khi object bật; bật/tắt bằng code (<see cref="StartPulse"/> / <see cref="StopPulse"/>).
    /// </summary>
    public class PulseScale : MonoBehaviour
    {
        [Tooltip("Biên độ pulse (0.1 = phình to thêm tối đa 10% rồi về gốc).")]
        public float amount = 0.1f;
        [Tooltip("Thời gian TRONG 1 chu kỳ pulse (giây).")]
        public float pulseDuration = 0.4f;
        [Tooltip("Số lần pulse TRONG 1 Pulse Duration. Vd 2 = trong 0.5s phình/về 2 lần (mỗi lần 0.25s).")]
        [Min(1)] public int pulsesPerCycle = 1;
        [Tooltip("Thời gian NGHỈ giữa 2 lần pulse (giây). 0 = pulse liên tục không nghỉ.")]
        public float gapBetween = 1f;
        [Tooltip("Tự chạy ngay khi object bật.")]
        public bool playOnEnable = true;
        [Tooltip("Dùng thời gian KHÔNG bị Time.timeScale ảnh hưởng (UI nên bật).")]
        public bool useUnscaledTime = true;

        private Vector3 _baseScale;
        private Coroutine _routine;

        private void Awake() => _baseScale = transform.localScale;

        private void OnEnable()
        {
            if (playOnEnable) StartPulse();
        }

        private void OnDisable()
        {
            // Trả về scale gốc để không bị kẹt ở giữa nhịp pulse.
            if (_routine != null) StopCoroutine(_routine);
            _routine = null;
            transform.localScale = _baseScale;
        }

        /// <summary>Bắt đầu pulse lặp.</summary>
        public void StartPulse()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(Loop());
        }

        /// <summary>Dừng pulse và trả scale về gốc.</summary>
        public void StopPulse()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = null;
            transform.localScale = _baseScale;
        }

        /// <summary>Cập nhật scale gốc (vd khi đổi kích thước runtime) để pulse tính lại từ đây.</summary>
        public void SetBaseScale(Vector3 baseScale) => _baseScale = baseScale;

        private float Dt => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        private IEnumerator Loop()
        {
            while (true)
            {
                // --- 1 chu kỳ: lặp envelope 0->1->0 (|sin|) N lần trong pulseDuration ---
                float dur = Mathf.Max(0.01f, pulseDuration);
                int n = Mathf.Max(1, pulsesPerCycle);
                float t = 0f;
                while (t < dur)
                {
                    t += Dt;
                    float k = Mathf.Abs(Mathf.Sin(Mathf.Clamp01(t / dur) * Mathf.PI * n));
                    transform.localScale = _baseScale * (1f + amount * k);
                    yield return null;
                }
                transform.localScale = _baseScale;

                // --- nghỉ giữa 2 lần ---
                float w = 0f;
                while (w < gapBetween)
                {
                    w += Dt;
                    yield return null;
                }
            }
        }
    }
}
