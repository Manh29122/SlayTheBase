using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SlayTheTower.Drawing
{
    /// <summary>
    /// Bề mặt vẽ trên UI: gắn lên một RawImage. Người chơi chạm/kéo để vẽ pixel.
    /// Quy đổi vị trí con trỏ -> ô pixel của texture, nội suy đoạn khi kéo nhanh.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class CardDrawingSurface : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        [Header("Độ phân giải pixel")]
        [SerializeField] private int width = 32;
        [SerializeField] private int height = 32;
        [Tooltip("Màu nền ban đầu (mặc định trong suốt để làm artwork thẻ).")]
        [SerializeField] private Color background = new Color(0, 0, 0, 0);

        [Header("Cọ")]
        public Color32 BrushColor = new Color32(0, 0, 0, 255);
        [Min(1)] public int BrushSize = 1;
        [Tooltip("Cỡ cọ tối đa khi điều khiển bằng Scrollbar/Slider (giá trị 0..1).")]
        [Min(1)] public int maxBrushSize = 10;

        private RawImage _image;
        private RectTransform _rect;
        private PixelPainter _painter;
        private int _lastX, _lastY;

        public Texture2D Texture => _painter?.Texture;

        private void Awake()
        {
            _image = GetComponent<RawImage>();
            _rect = (RectTransform)transform;
            EnsurePainter();
        }

        private void EnsurePainter()
        {
            if (_painter != null) return;
            _painter = new PixelPainter(width, height, background);
            _image.texture = _painter.Texture;
        }

        /// <summary>Khởi tạo lại với độ phân giải / nền khác (tuỳ chọn).</summary>
        public void Initialize(int w, int h, Color bg)
        {
            width = w; height = h; background = bg;
            _painter = new PixelPainter(width, height, bg);
            if (_image == null) _image = GetComponent<RawImage>();
            _image.texture = _painter.Texture;
        }

        public void OnPointerDown(PointerEventData e)
        {
            if (!TryPixel(e, out int x, out int y)) return;
            _painter.PaintLine(x, y, x, y, BrushColor, BrushSize);
            _lastX = x; _lastY = y;
        }

        public void OnDrag(PointerEventData e)
        {
            if (!TryPixel(e, out int x, out int y)) return;
            _painter.PaintLine(_lastX, _lastY, x, y, BrushColor, BrushSize);
            _lastX = x; _lastY = y;
        }

        private bool TryPixel(PointerEventData e, out int px, out int py)
        {
            px = py = 0;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rect, e.position, e.pressEventCamera, out var local))
                return false;

            var rect = _rect.rect;
            float u = (local.x - rect.x) / rect.width;
            float v = (local.y - rect.y) / rect.height;
            if (u < 0f || u > 1f || v < 0f || v > 1f) return false;

            px = Mathf.Clamp((int)(u * width), 0, width - 1);
            py = Mathf.Clamp((int)(v * height), 0, height - 1);
            return true;
        }

        public void Clear() => _painter.Fill(background);

        public byte[] EncodePng() => _painter.EncodePng();

        // ----- Hàm tiện ích để nối thẳng vào Button trong Inspector (không cần viết code) -----

        /// <summary>Đổi màu cọ (Button.onClick không truyền được Color → dùng DrawColorButton cho nút màu).</summary>
        public void SetBrushColor(Color color) => BrushColor = color;

        /// <summary>Đổi cỡ cọ (Button.onClick điền được tham số int trong Inspector).</summary>
        public void SetBrushSize(int size) => BrushSize = Mathf.Max(1, size);

        /// <summary>
        /// Đặt cỡ cọ từ giá trị 0..1 (vd Scrollbar) → quy đổi sang 1..<see cref="maxBrushSize"/>.
        /// Nối Scrollbar.onValueChanged → hàm này (Dynamic float).
        /// </summary>
        public void SetBrushSizeNormalized(float t)
        {
            BrushSize = Mathf.Max(1, Mathf.RoundToInt(Mathf.Lerp(1f, maxBrushSize, Mathf.Clamp01(t))));
        }

        /// <summary>Bật tẩy (vẽ màu trong suốt).</summary>
        public void UseEraser() => BrushColor = new Color32(0, 0, 0, 0);

        /// <summary>Lưu ảnh ra PNG; trả về id (tên file). id rỗng = tự đặt theo thời gian.</summary>
        public string Save(string id = null)
        {
            // Hậu tố ngẫu nhiên để KHÔNG trùng tên dù lưu nhiều lần trong cùng 1 giây.
            if (string.IsNullOrEmpty(id))
                id = "card_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + Guid.NewGuid().ToString("N")[..6];
            CardArtStorage.Save(id, EncodePng());
            return id;
        }

        /// <summary>Lưu nhanh (id tự đặt) — nối thẳng vào Button.onClick.</summary>
        public void SaveAuto() => Save();

        /// <summary>Nạp một ảnh ĐÃ LƯU (theo id) vào khung để xem/vẽ tiếp. Trả về false nếu không có file.</summary>
        public bool LoadFromSaved(string id)
        {
            var tex = CardArtStorage.LoadTexture(id);
            if (tex == null) return false;

            EnsurePainter();
            if (tex.width != width || tex.height != height)
                Initialize(tex.width, tex.height, background); // khớp độ phân giải ảnh
            _painter.LoadPixels(tex.GetPixels32(), tex.width, tex.height);
            return true;
        }

        /// <summary>Tạo Sprite từ ảnh đang vẽ (point filter, để hiển thị trên thẻ).</summary>
        public Sprite ToSprite()
        {
            var tex = _painter.Texture;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), Mathf.Max(tex.width, tex.height));
        }
    }
}
