using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Bảng màu dạng VÒNG TRÒN (HSV): góc = Hue, bán kính = Saturation, độ sáng = Value.
    /// Gắn lên một RawImage; chạm/kéo trong vòng để chọn màu. Bắn sự kiện OnColorChanged.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class ColorWheel : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        [SerializeField] private int resolution = 128;
        [Range(0f, 1f)] public float Value = 1f;

        [Tooltip("Bắn màu khi chọn — nối thẳng tới CardDrawingSurface.SetBrushColor (chọn 'Dynamic Color').")]
        public UnityEvent<Color> onColorPicked = new();

        public event Action<Color> OnColorChanged;
        public Color SelectedColor { get; private set; } = Color.white;

        private RawImage _image;
        private RectTransform _rect;
        private float _hue;
        private float _sat;

        private void Awake()
        {
            _image = GetComponent<RawImage>();
            _rect = (RectTransform)transform;
            _image.texture = GenerateWheel(Mathf.Max(16, resolution));
        }

        private static Texture2D GenerateWheel(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var px = new Color32[size * size];
            float r = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - r + 0.5f;
                    float dy = y - r + 0.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist <= r)
                    {
                        float h = Mathf.Atan2(dy, dx) / (2f * Mathf.PI) + 0.5f; // 0..1
                        float s = Mathf.Clamp01(dist / r);
                        var col = (Color32)Color.HSVToRGB(h, s, 1f);
                        // làm mềm viền ngoài 1px cho đỡ răng cưa
                        byte a = (byte)(dist > r - 1f ? Mathf.Lerp(255f, 0f, dist - (r - 1f)) : 255f);
                        col.a = a;
                        px[y * size + x] = col;
                    }
                    else px[y * size + x] = new Color32(0, 0, 0, 0);
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            return tex;
        }

        public void OnPointerDown(PointerEventData e) => Pick(e);
        public void OnDrag(PointerEventData e) => Pick(e);

        private void Pick(PointerEventData e)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rect, e.position, e.pressEventCamera, out var local))
                return;

            var rect = _rect.rect;
            Vector2 center = rect.center;
            float dx = local.x - center.x;
            float dy = local.y - center.y;
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            _hue = Mathf.Atan2(dy, dx) / (2f * Mathf.PI) + 0.5f;
            _sat = radius > 0f ? Mathf.Clamp01(dist / radius) : 0f;
            Emit();
        }

        /// <summary>Đổi độ sáng (Value) mà vẫn giữ hue/sat đang chọn.</summary>
        public void SetValue(float value)
        {
            Value = Mathf.Clamp01(value);
            Emit();
        }

        private void Emit()
        {
            SelectedColor = Color.HSVToRGB(_hue, _sat, Value);
            OnColorChanged?.Invoke(SelectedColor);
            onColorPicked?.Invoke(SelectedColor);
        }
    }
}
