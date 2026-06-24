using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using SlayTheTower.Drawing;

namespace SlayTheTower
{
    /// <summary>
    /// BOOTSTRAP DEMO: vẽ pixel lên "lá bài trắng" rồi lưu thành PNG (persistentDataPath/cards).
    /// Bấm Play → chọn màu, vẽ bằng chuột/chạm, đổi cỡ cọ, Eraser/Clear, Save.
    /// Sau khi Save sẽ nạp lại ảnh từ ổ đĩa và hiển thị ở ô preview để kiểm chứng.
    /// </summary>
    public class CardDrawDemo : MonoBehaviour
    {
        private CardDrawingSurface _surface;
        private RawImage _preview;
        private Text _pathLabel;

        private static readonly (string name, Color32 col)[] Palette =
        {
            ("Đen", new Color32(0, 0, 0, 255)),
            ("Trắng", new Color32(255, 255, 255, 255)),
            ("Đỏ", new Color32(230, 60, 60, 255)),
            ("Cam", new Color32(240, 150, 40, 255)),
            ("Vàng", new Color32(245, 220, 60, 255)),
            ("Lục", new Color32(70, 200, 90, 255)),
            ("Lam", new Color32(60, 140, 230, 255)),
            ("Tím", new Color32(160, 90, 220, 255)),
            ("Nâu", new Color32(140, 90, 50, 255)),
        };

        private void Awake()
        {
            EnsureEventSystem();
            var canvas = BuildCanvas();

            AddLabel(canvas.transform, "VẼ LÁ BÀI — chọn màu, vẽ, rồi Save", new Vector2(0, 1),
                new Vector2(0, -16), 30, TextAnchor.UpperCenter, 700);

            // Nền xám sau bề mặt vẽ (để thấy vùng trong suốt) + bề mặt vẽ
            var backing = CreatePanel("Backing", canvas.transform, new Color(0.5f, 0.5f, 0.5f),
                new Vector2(0, 70), new Vector2(560, 560), false);
            var surfGO = new GameObject("DrawSurface", typeof(RawImage), typeof(CardDrawingSurface));
            surfGO.transform.SetParent(backing.transform, false);
            var srt = (RectTransform)surfGO.transform;
            srt.anchorMin = Vector2.zero; srt.anchorMax = Vector2.one;
            srt.offsetMin = Vector2.zero; srt.offsetMax = Vector2.zero;
            _surface = surfGO.GetComponent<CardDrawingSurface>();

            // Hàng bảng màu
            BuildPalette(canvas.transform, -260f);

            // Hàng công cụ
            BuildTools(canvas.transform, -340f);

            // Ô preview ảnh đã lưu
            CreatePanel("PreviewBg", canvas.transform, new Color(0.2f, 0.2f, 0.2f),
                new Vector2(720, 220), new Vector2(176, 176), false);
            var prevGO = new GameObject("Preview", typeof(RawImage));
            prevGO.transform.SetParent(canvas.transform, false);
            var prt = (RectTransform)prevGO.transform;
            prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.5f);
            prt.anchoredPosition = new Vector2(720, 220);
            prt.sizeDelta = new Vector2(160, 160);
            _preview = prevGO.GetComponent<RawImage>();
            AddLabel(canvas.transform, "Preview (đã lưu)", new Vector2(0.5f, 0.5f),
                new Vector2(720, 320), 22, TextAnchor.MiddleCenter, 240);

            _pathLabel = AddLabel(canvas.transform, "", new Vector2(0.5f, 0f),
                new Vector2(0, 24), 20, TextAnchor.LowerCenter, 1400);
        }

        // ---------- Bảng màu & công cụ ----------
        private void BuildPalette(Transform parent, float y)
        {
            int n = Palette.Length;
            const float w = 64f, gap = 72f;
            float startX = -(n - 1) * gap * 0.5f;
            for (int i = 0; i < n; i++)
            {
                var col = Palette[i].col;
                CreateButtonAt(parent, "", col, new Vector2(startX + i * gap, y), new Vector2(w, w),
                    () => _surface.BrushColor = col);
            }
        }

        private void BuildTools(Transform parent, float y)
        {
            const float w = 130f, gap = 140f;
            string[] labels = { "Eraser", "Cọ 1", "Cọ 2", "Cọ 4", "Clear", "SAVE" };
            float startX = -(labels.Length - 1) * gap * 0.5f;

            CreateButtonAt(parent, "Eraser", new Color(0.85f, 0.85f, 0.85f), new Vector2(startX + 0 * gap, y),
                new Vector2(w, 46), () => _surface.BrushColor = new Color32(0, 0, 0, 0));
            CreateButtonAt(parent, "Cọ 1", new Color(0.8f, 0.8f, 0.8f), new Vector2(startX + 1 * gap, y),
                new Vector2(w, 46), () => _surface.BrushSize = 1);
            CreateButtonAt(parent, "Cọ 2", new Color(0.8f, 0.8f, 0.8f), new Vector2(startX + 2 * gap, y),
                new Vector2(w, 46), () => _surface.BrushSize = 2);
            CreateButtonAt(parent, "Cọ 4", new Color(0.8f, 0.8f, 0.8f), new Vector2(startX + 3 * gap, y),
                new Vector2(w, 46), () => _surface.BrushSize = 4);
            CreateButtonAt(parent, "Clear", new Color(0.9f, 0.7f, 0.5f), new Vector2(startX + 4 * gap, y),
                new Vector2(w, 46), () => _surface.Clear());
            CreateButtonAt(parent, "SAVE", new Color(0.5f, 0.85f, 0.6f), new Vector2(startX + 5 * gap, y),
                new Vector2(w, 46), SaveDrawing);
        }

        private void SaveDrawing()
        {
            string fileName = "card_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string path = CardArtStorage.Save(fileName, _surface.EncodePng());

            // Nạp lại từ ổ đĩa để kiểm chứng round-trip, hiển thị ở preview.
            var sprite = CardArtStorage.Load(fileName);
            if (sprite != null) _preview.texture = sprite.texture;
            _pathLabel.text = "Đã lưu: " + path;
            Debug.Log("[CardDraw] Saved: " + path);
        }

        // ---------- Helpers UI ----------
        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }

        private static Canvas BuildCanvas()
        {
            var go = new GameObject("DrawCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static Image CreatePanel(string n, Transform parent, Color color,
            Vector2 anchoredPos, Vector2 size, bool raycast)
        {
            var go = new GameObject(n, typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.color = color; img.raycastTarget = raycast;
            return img;
        }

        private static void CreateButtonAt(Transform parent, string label, Color color,
            Vector2 anchoredPos, Vector2 size, Action onClick)
        {
            var go = new GameObject($"Btn_{label}", typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = color;
            go.GetComponent<Button>().onClick.AddListener(() => onClick());

            if (!string.IsNullOrEmpty(label))
            {
                var t = AddLabel(go.transform, label, new Vector2(0.5f, 0.5f), Vector2.zero, 22,
                    TextAnchor.MiddleCenter, size.x);
                t.color = Color.black;
            }
        }

        private static Text AddLabel(Transform parent, string text, Vector2 anchor,
            Vector2 anchoredPos, int fontSize, TextAnchor align, float width)
        {
            var go = new GameObject("Label", typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(width, fontSize + 12);
            var txt = go.GetComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = text; txt.alignment = align; txt.fontSize = fontSize;
            txt.color = Color.white; txt.raycastTarget = false;
            return txt;
        }
    }
}
