using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using SlayTheTower.Data;
using SlayTheTower.Drawing;
using SlayTheTower.UI;

namespace SlayTheTower
{
    /// <summary>
    /// BOOTSTRAP DEMO "Card Creator": người chơi chọn ảnh nền, gõ TÊN + MÔ TẢ (chọn màu chữ),
    /// vẽ ARTWORK, đặt COST → xem preview lá bài cập nhật trực tiếp → Save (PNG + JSON).
    /// "Load Last" dựng lại lá vừa lưu từ dữ liệu để chứng minh ghép lại đúng.
    /// </summary>
    public class CardCreatorDemo : MonoBehaviour
    {
        private CardDrawingSurface _surface;
        private CardView _preview;
        private CardDefinition _previewDef;
        private Image _previewBg;
        private Text _previewNameText;
        private Text _previewDescText;

        private readonly List<(string id, Sprite sprite)> _backgrounds = new();
        private int _bgIndex;
        private int _cost = 3;
        private string _name = "";
        private string _desc = "";
        private Color _nameColor = Color.white;
        private Color _descColor = new Color(0.6f, 0.85f, 1f);

        private Text _bgNameLabel;
        private Text _costLabel;
        private Text _statusLabel;

        private ColorWheel _wheel;
        private Image _currentColorSwatch;
        private int _colorTarget; // 0 = Tên, 1 = Mô tả, 2 = Cọ

        private void Awake()
        {
            EnsureEventSystem();
            var canvas = BuildCanvas();
            BuildBackgrounds();

            AddLabel(canvas.transform, "CARD CREATOR", new Vector2(0.5f, 1f), new Vector2(0, -14), 30,
                TextAnchor.UpperCenter, 600);

            // Khung vẽ artwork (trái)
            var backing = CreatePanel("Backing", canvas.transform, new Color(0.5f, 0.5f, 0.5f),
                new Vector2(-560, 70), new Vector2(440, 440), false);
            var surfGO = new GameObject("DrawSurface", typeof(RawImage), typeof(CardDrawingSurface));
            surfGO.transform.SetParent(backing.transform, false);
            var srt = (RectTransform)surfGO.transform;
            srt.anchorMin = Vector2.zero; srt.anchorMax = Vector2.one;
            srt.offsetMin = Vector2.zero; srt.offsetMax = Vector2.zero;
            _surface = surfGO.GetComponent<CardDrawingSurface>();
            BuildDrawPalette(canvas.transform, -560f, -190f);

            // Preview (phải)
            _preview = BuildPreviewCard(canvas.transform, new Vector2(640, 40), new Vector2(320, 440));
            _previewDef = ScriptableObject.CreateInstance<CardDefinition>();
            _previewDef.artwork = _surface.ToSprite(); // sprite bám texture đang vẽ -> preview cập nhật live

            // Controls (giữa)
            BuildControls(canvas.transform);
            BuildColorPicker(canvas.transform);

            ApplyToPreview();
        }

        // ---------- Controls ----------
        private void BuildControls(Transform p)
        {
            // Background picker
            AddLabel(p, "Nền:", new Vector2(0.5f, 0.5f), new Vector2(-150, 380), 22, TextAnchor.MiddleLeft, 120);
            CreateButton(p, "◀", new Vector2(-150, 340), new Vector2(40, 40), () => CycleBg(-1));
            _bgNameLabel = AddLabel(p, "", new Vector2(0.5f, 0.5f), new Vector2(40, 340), 22, TextAnchor.MiddleCenter, 200);
            CreateButton(p, "▶", new Vector2(170, 340), new Vector2(40, 40), () => CycleBg(1));

            // Name
            AddLabel(p, "Tên:", new Vector2(0.5f, 0.5f), new Vector2(-150, 288), 22, TextAnchor.MiddleLeft, 120);
            CreateInput(p, new Vector2(110, 288), new Vector2(330, 42), "Tên lá bài...", false, s => { _name = s; ApplyToPreview(); });

            // Description
            AddLabel(p, "Mô tả:", new Vector2(0.5f, 0.5f), new Vector2(-150, 188), 22, TextAnchor.MiddleLeft, 120);
            CreateInput(p, new Vector2(110, 185), new Vector2(330, 78), "Mô tả...", true, s => { _desc = s; ApplyToPreview(); });

            // Cost
            AddLabel(p, "Cost:", new Vector2(0.5f, 0.5f), new Vector2(-150, 60), 22, TextAnchor.MiddleLeft, 120);
            CreateButton(p, "−", new Vector2(-40, 60), new Vector2(40, 40), () => SetCost(_cost - 1));
            _costLabel = AddLabel(p, "3", new Vector2(0.5f, 0.5f), new Vector2(20, 60), 24, TextAnchor.MiddleCenter, 50);
            CreateButton(p, "+", new Vector2(80, 60), new Vector2(40, 40), () => SetCost(_cost + 1));

            // Save / Load
            CreateButton(p, "SAVE", new Vector2(40, 0), new Vector2(170, 46), Save, new Color(0.5f, 0.85f, 0.6f));
            CreateButton(p, "LOAD LAST", new Vector2(40, -56), new Vector2(170, 46), LoadLast, new Color(0.6f, 0.8f, 0.95f));
            _statusLabel = AddLabel(p, "", new Vector2(0.5f, 0f), new Vector2(0, 24), 18, TextAnchor.LowerCenter, 1500);
        }

        // ---------- Bảng màu dạng vòng (wheel) ----------
        private void BuildColorPicker(Transform parent)
        {
            AddLabel(parent, "Màu (chọn trên vòng):", new Vector2(0.5f, 0.5f), new Vector2(640, -195), 22,
                TextAnchor.MiddleCenter, 320);

            // Áp màu cho phần nào
            CreateButton(parent, "Tên", new Vector2(555, -230), new Vector2(82, 36), () => SetColorTarget(0));
            CreateButton(parent, "Mô tả", new Vector2(642, -230), new Vector2(82, 36), () => SetColorTarget(1));
            CreateButton(parent, "Cọ", new Vector2(729, -230), new Vector2(82, 36), () => SetColorTarget(2));

            // Vòng màu
            var go = new GameObject("ColorWheel", typeof(RawImage), typeof(ColorWheel));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(615, -345);
            rt.sizeDelta = new Vector2(170, 170);
            _wheel = go.GetComponent<ColorWheel>();
            _wheel.OnColorChanged += OnWheelColor;

            // Ô màu đang chọn
            _currentColorSwatch = CreatePanel("CurColor", parent, _nameColor, new Vector2(745, -345), new Vector2(44, 44), false);

            // Độ sáng (Value)
            CreateButton(parent, "Sáng", new Vector2(555, -455), new Vector2(82, 32), () => _wheel.SetValue(1f));
            CreateButton(parent, "Vừa", new Vector2(642, -455), new Vector2(82, 32), () => _wheel.SetValue(0.65f));
            CreateButton(parent, "Tối", new Vector2(729, -455), new Vector2(82, 32), () => _wheel.SetValue(0.35f));

            SetColorTarget(0);
        }

        private void SetColorTarget(int t)
        {
            _colorTarget = t;
            Color c = t == 1 ? _descColor : t == 2 ? (Color)_surface.BrushColor : _nameColor;
            if (_currentColorSwatch != null) _currentColorSwatch.color = c;
        }

        private void OnWheelColor(Color c)
        {
            if (_currentColorSwatch != null) _currentColorSwatch.color = c;
            switch (_colorTarget)
            {
                case 0: _nameColor = c; ApplyToPreview(); break;
                case 1: _descColor = c; ApplyToPreview(); break;
                case 2: _surface.BrushColor = c; break;
            }
        }

        private void CycleBg(int dir)
        {
            if (_backgrounds.Count == 0) return;
            _bgIndex = (_bgIndex + dir + _backgrounds.Count) % _backgrounds.Count;
            ApplyToPreview();
        }

        private void SetCost(int v)
        {
            _cost = Mathf.Clamp(v, 0, 99);
            if (_costLabel != null) _costLabel.text = _cost.ToString();
            ApplyToPreview();
        }

        private void ApplyToPreview()
        {
            _previewDef.displayName = _name;
            _previewDef.description = _desc;
            _previewDef.energyCost = _cost;
            _preview.SetDefinition(_previewDef); // đổ text/cost/artwork

            // Nền + màu chữ: áp TRỰC TIẾP lên UI preview (CardDefinition không còn giữ styling).
            if (_previewNameText != null) _previewNameText.color = _nameColor;
            if (_previewDescText != null) _previewDescText.color = _descColor;
            if (_backgrounds.Count > 0)
            {
                if (_previewBg != null) _previewBg.sprite = _backgrounds[_bgIndex].sprite;
                if (_bgNameLabel != null) _bgNameLabel.text = _backgrounds[_bgIndex].id;
            }
        }

        // ---------- Lưu / nạp ----------
        private void Save()
        {
            string id = "card_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            CardArtStorage.Save(id, _surface.EncodePng());

            var data = new PlayerCardData
            {
                id = id,
                displayName = _name,
                description = _desc,
                energyCost = _cost,
                cardType = CardType.SummonUnit,
                backgroundId = _backgrounds.Count > 0 ? _backgrounds[_bgIndex].id : "",
                nameColor = _nameColor,
                descColor = _descColor,
                artFile = id
            };
            PlayerCardStorage.Add(data);
            _statusLabel.text = "Đã lưu lá: " + id + "  (" + CardArtStorage.Folder + ")";
            Debug.Log("[CardCreator] Saved " + id);
        }

        private void LoadLast()
        {
            var all = PlayerCardStorage.LoadAll();
            if (all.Count == 0) { _statusLabel.text = "Chưa có lá nào được lưu."; return; }

            var data = all[all.Count - 1];
            // Ghép lại từ dữ liệu — đúng luồng mobile: KHÔNG prefab, dựng CardDefinition trong RAM.
            var rebuilt = RuntimeCardFactory.Build(data, null);
            _preview.SetDefinition(rebuilt);
            // Styling (nền/màu) áp trực tiếp lên UI từ dữ liệu đã lưu.
            if (_previewNameText != null) _previewNameText.color = data.nameColor;
            if (_previewDescText != null) _previewDescText.color = data.descColor;
            if (_previewBg != null) _previewBg.sprite = BgLookup(data.backgroundId);
            _statusLabel.text = "Đã nạp & ghép lại: " + data.id;
        }

        private Sprite BgLookup(string id)
        {
            foreach (var b in _backgrounds) if (b.id == id) return b.sprite;
            return null;
        }

        // ---------- Background giả lập (game thật dùng BackgroundLibrary SO) ----------
        private void BuildBackgrounds()
        {
            AddBg("bg_blue", new Color(0.55f, 0.8f, 1f), new Color(0.1f, 0.3f, 0.6f));
            AddBg("bg_red", new Color(1f, 0.7f, 0.65f), new Color(0.6f, 0.15f, 0.15f));
            AddBg("bg_green", new Color(0.7f, 1f, 0.75f), new Color(0.15f, 0.5f, 0.2f));
            AddBg("bg_gold", new Color(1f, 0.92f, 0.6f), new Color(0.6f, 0.45f, 0.1f));
        }

        private void AddBg(string id, Color fill, Color border)
        {
            const int w = 64, h = 90, b = 4;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    px[y * w + x] = (x < b || y < b || x >= w - b || y >= h - b) ? (Color32)border : (Color32)fill;
            tex.SetPixels32(px); tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16);
            _backgrounds.Add((id, sprite));
        }

        private void BuildDrawPalette(Transform p, float cx, float y)
        {
            Color32[] cols =
            {
                new Color32(0,0,0,255), new Color32(255,255,255,255), new Color32(230,60,60,255),
                new Color32(245,220,60,255), new Color32(70,200,90,255), new Color32(60,140,230,255)
            };
            float startX = cx - (cols.Length - 1) * 56f * 0.5f;
            for (int i = 0; i < cols.Length; i++)
            {
                var c = cols[i];
                CreateButton(p, "", new Vector2(startX + i * 56f, y), new Vector2(48, 48), () => _surface.BrushColor = c, (Color)c);
            }
            CreateButton(p, "Eraser", new Color(0.85f, 0.85f, 0.85f), new Vector2(cx - 90, y - 56), new Vector2(110, 40),
                () => _surface.BrushColor = new Color32(0, 0, 0, 0));
            CreateButton(p, "Clear", new Color(0.9f, 0.7f, 0.5f), new Vector2(cx + 60, y - 56), new Vector2(110, 40),
                () => _surface.Clear());
        }

        // ---------- Preview card ----------
        private CardView BuildPreviewCard(Transform parent, Vector2 pos, Vector2 size)
        {
            var root = new GameObject("PreviewCard", typeof(Image), typeof(CardView));
            root.transform.SetParent(parent, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            var rootImg = root.GetComponent<Image>();
            rootImg.color = new Color(0, 0, 0, 0); rootImg.raycastTarget = false;

            var bg = ChildImage(root.transform, "Background", Vector2.zero, Vector2.one);
            var art = ChildImage(root.transform, "ArtWork", new Vector2(0.13f, 0.46f), new Vector2(0.87f, 0.82f));
            art.color = Color.white;
            var nameT = ChildText(root.transform, "NameText", new Vector2(0.08f, 0.37f), new Vector2(0.92f, 0.45f), 22, TextAnchor.MiddleCenter);
            var descT = ChildText(root.transform, "DescriptionText", new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.35f), 18, TextAnchor.UpperCenter);
            var costT = ChildText(root.transform, "CostText", new Vector2(0.74f, 0.86f), new Vector2(0.95f, 0.98f), 28, TextAnchor.MiddleCenter);

            var cv = root.GetComponent<CardView>();
            cv.BindUI(bg, art, nameT, costT, descT);
            _previewBg = bg; _previewNameText = nameT; _previewDescText = descT;
            return cv;
        }

        private static Image ChildImage(Transform parent, string n, Vector2 aMin, Vector2 aMax)
        {
            var go = new GameObject(n, typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>(); img.raycastTarget = false;
            return img;
        }

        private static Text ChildText(Transform parent, string n, Vector2 aMin, Vector2 aMax, int fontSize, TextAnchor align)
        {
            var go = new GameObject(n, typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var t = go.GetComponent<Text>();
            t.font = UIFont(); t.fontSize = fontSize; t.alignment = align; t.color = Color.white;
            t.raycastTarget = false; t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Truncate;
            return t;
        }

        // ---------- Helpers UI ----------
        private static Font UIFont() => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }

        private static Canvas BuildCanvas()
        {
            var go = new GameObject("CreatorCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 100;
            var s = go.GetComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1920, 1080); s.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static Image CreatePanel(string n, Transform parent, Color color, Vector2 pos, Vector2 size, bool raycast)
        {
            var go = new GameObject(n, typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size;
            var img = go.GetComponent<Image>(); img.color = color; img.raycastTarget = raycast;
            return img;
        }

        private static void CreateButton(Transform parent, string label, Vector2 pos, Vector2 size, Action onClick, Color? bg = null)
            => CreateButton(parent, label, bg ?? new Color(0.8f, 0.8f, 0.8f), pos, size, onClick);

        private static void CreateButton(Transform parent, string label, Color bg, Vector2 pos, Vector2 size, Action onClick)
        {
            var go = new GameObject("Btn", typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size;
            go.GetComponent<Image>().color = bg;
            go.GetComponent<Button>().onClick.AddListener(() => onClick());
            if (!string.IsNullOrEmpty(label))
            {
                var t = AddLabel(go.transform, label, new Vector2(0.5f, 0.5f), Vector2.zero, 22, TextAnchor.MiddleCenter, size.x);
                t.color = Color.black;
            }
        }

        private static InputField CreateInput(Transform parent, Vector2 pos, Vector2 size, string placeholder, bool multiline, Action<string> onChanged)
        {
            var go = new GameObject("Input", typeof(Image), typeof(InputField));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size;
            go.GetComponent<Image>().color = Color.white;
            var input = go.GetComponent<InputField>();

            var text = ChildPaddedText(go.transform, "Text", Color.black, multiline);
            var ph = ChildPaddedText(go.transform, "Placeholder", new Color(0.5f, 0.5f, 0.5f), multiline);
            ph.text = placeholder; ph.fontStyle = FontStyle.Italic;

            input.textComponent = text; input.placeholder = ph;
            input.lineType = multiline ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;
            input.onValueChanged.AddListener(v => onChanged(v));
            return input;
        }

        private static Text ChildPaddedText(Transform parent, string n, Color color, bool multiline)
        {
            var go = new GameObject(n, typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(8, 4); rt.offsetMax = new Vector2(-8, -4);
            var t = go.GetComponent<Text>();
            t.font = UIFont(); t.color = color; t.fontSize = 22; t.supportRichText = false;
            t.alignment = multiline ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft;
            return t;
        }

        private static Text AddLabel(Transform parent, string text, Vector2 anchor, Vector2 pos, int fontSize, TextAnchor align, float width)
        {
            var go = new GameObject("Label", typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = anchor; rt.pivot = anchor; rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(width, fontSize + 14);
            var t = go.GetComponent<Text>();
            t.font = UIFont(); t.text = text; t.alignment = align; t.fontSize = fontSize;
            t.color = Color.white; t.raycastTarget = false;
            return t;
        }
    }
}
