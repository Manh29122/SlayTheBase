using UnityEngine;
using UnityEngine.UI;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Viền chạy quanh lá bài (shader SlayTheTower/CardRainbowBorder). Mặc định ẩn; bật khi lá được chọn.
    /// Mỗi lá dùng 1 bản material RIÊNG để chỉnh màu/cầu vồng độc lập.
    /// Gắn lên lá bài; gán <see cref="borderImage"/> = Image phủ viền (có material rainbow).
    /// </summary>
    public class CardSelectionBorder : MonoBehaviour
    {
        [Tooltip("Image phủ viền (size = lá bài), gán material SlayTheTower/CardRainbowBorder.")]
        [SerializeField] private Image borderImage;
        [Tooltip("Dùng cầu vồng 7 màu (true) hay 1 màu cố định (false).")]
        [SerializeField] private bool useRainbow = true;
        [Tooltip("Màu viền khi không dùng cầu vồng.")]
        [SerializeField] private Color borderColor = Color.white;
        [Tooltip("Hiện viền ngay cả khi không chọn (vd để xem trước).")]
        [SerializeField] private bool alwaysOn = false;

        private Material _mat;

        private void Awake()
        {
            if (borderImage != null && borderImage.material != null)
            {
                _mat = Instantiate(borderImage.material); // bản riêng từng lá
                borderImage.material = _mat;
                Apply();
            }
            SetSelected(alwaysOn);
        }

        /// <summary>Bật/tắt viền (hand gọi khi chọn/bỏ chọn lá).</summary>
        public void SetSelected(bool on)
        {
            if (borderImage != null) borderImage.enabled = on || alwaysOn;
        }

        /// <summary>Bật/tắt chế độ cầu vồng.</summary>
        public void SetRainbow(bool on)
        {
            useRainbow = on;
            Apply();
        }

        /// <summary>Đặt màu viền (tự chuyển sang chế độ màu đơn).</summary>
        public void SetColor(Color c)
        {
            borderColor = c;
            useRainbow = false;
            Apply();
        }

        private void Apply()
        {
            if (_mat == null) return;
            _mat.SetFloat("_UseRainbow", useRainbow ? 1f : 0f);
            _mat.SetColor("_Color", borderColor);

            // Set tỉ lệ W/H để góc bo tròn đều (không méo trên lá hình chữ nhật).
            if (borderImage != null)
            {
                var r = borderImage.rectTransform.rect;
                if (r.height > 0.0001f) _mat.SetFloat("_Aspect", r.width / r.height);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying && _mat != null) Apply();
        }
#endif
    }
}
