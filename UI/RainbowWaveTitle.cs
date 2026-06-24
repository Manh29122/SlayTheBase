using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Tiêu đề kiểu cầu vồng (TextMeshPro): mỗi chữ là 1 entry trong list (ký tự + màu riêng),
    /// xếp theo CUNG CONG, và chạy hiệu ứng SÓNG — từng chữ phóng to/thu nhỏ lần lượt rồi lặp lại.
    /// Gắn lên một RectTransform con của Canvas (vị trí = tâm tiêu đề).
    /// </summary>
    public class RainbowWaveTitle : MonoBehaviour
    {
        [System.Serializable]
        public struct Letter
        {
            public string character;
            public Color color;
        }

        [Header("Chữ (đọc theo thứ tự list)")]
        [SerializeField] private List<Letter> letters = new();
        [SerializeField] private float fontSize = 64f;
        [SerializeField] private bool bold = true;
        [Tooltip("Font TMP (để trống = dùng font mặc định trong TMP Settings).")]
        [SerializeField] private TMP_FontAsset fontAsset;

        [Header("Cung cong (rainbow)")]
        [Tooltip("Góc lệch mỗi chữ (độ). Lớn hơn = cong hơn.")]
        [SerializeField] private float anglePerLetter = 7f;
        [Tooltip("Bán kính cung. Lớn hơn = ít cong + chữ thưa hơn.")]
        [SerializeField] private float radius = 520f;
        [Tooltip("Xoay chữ theo tiếp tuyến cung.")]
        [SerializeField] private bool tiltAlongArc = true;

        [Header("Hiệu ứng sóng (scale)")]
        [Tooltip("Độ phóng to thêm (0.5 = +50%).")]
        [SerializeField] private float amplitude = 0.5f;
        [Tooltip("Tốc độ sóng.")]
        [SerializeField] private float waveSpeed = 6f;
        [Tooltip("Độ trễ pha giữa các chữ (sóng lan dần).")]
        [SerializeField] private float perLetterDelay = 0.55f;
        [Tooltip("Độ 'đột ngột' của cú pop (càng lớn càng gắt).")]
        [SerializeField] private float sharpness = 4f;

        [Header("Tiện ích")]
        [Tooltip("Bấm chuột phải component → 'Tách chữ...' để tự tạo list từ chuỗi này.")]
        [SerializeField] private string sourceText = "Slay The Base";

        private readonly List<RectTransform> _letterRects = new();

        private void Start() => Build();

        public void Build()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
            _letterRects.Clear();

            int n = letters.Count;
            float center = (n - 1) * 0.5f;

            for (int i = 0; i < n; i++)
            {
                var L = letters[i];
                var go = new GameObject($"Letter_{i}", typeof(TextMeshProUGUI));
                go.transform.SetParent(transform, false);

                var rt = (RectTransform)go.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(fontSize * 1.6f, fontSize * 1.8f);

                float d = i - center;
                float rad = d * anglePerLetter * Mathf.Deg2Rad;
                rt.anchoredPosition = new Vector2(radius * Mathf.Sin(rad), radius * Mathf.Cos(rad) - radius);
                if (tiltAlongArc) rt.localRotation = Quaternion.Euler(0f, 0f, -d * anglePerLetter);

                var tmp = go.GetComponent<TextMeshProUGUI>();
                if (fontAsset != null) tmp.font = fontAsset;
                tmp.text = L.character;
                tmp.color = L.color;
                tmp.fontSize = fontSize;
                tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;

                _letterRects.Add(rt);
            }
        }

        private void Update()
        {
            for (int i = 0; i < _letterRects.Count; i++)
            {
                if (_letterRects[i] == null) continue;
                float phase = Time.time * waveSpeed - i * perLetterDelay;
                float pulse = Mathf.Pow(Mathf.Clamp01(Mathf.Sin(phase)), Mathf.Max(1f, sharpness));
                float s = 1f + amplitude * pulse;
                _letterRects[i].localScale = new Vector3(s, s, 1f);
            }
        }

        [ContextMenu("Tách chữ từ Source Text (màu cầu vồng)")]
        private void FillFromSource()
        {
            letters = new List<Letter>();
            int len = sourceText.Length;
            for (int i = 0; i < len; i++)
            {
                float h = len > 1 ? (float)i / (len - 1) : 0f;
                letters.Add(new Letter
                {
                    character = sourceText[i].ToString(),
                    color = Color.HSVToRGB(h, 0.85f, 1f)
                });
            }
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
