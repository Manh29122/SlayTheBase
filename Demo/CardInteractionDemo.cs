using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using SlayTheTower.UI;

namespace SlayTheTower
{
    /// <summary>
    /// BOOTSTRAP DEMO cho tương tác thẻ bài.
    /// Tự dựng: Canvas + EventSystem + vùng vàng (trống) + vùng xanh (apply) + vùng đỏ (hand) + các lá bài.
    ///
    /// Cách dùng: tạo GameObject rỗng, gắn script này, bấm Play.
    ///  - Chạm lá bài  -> phóng to.
    ///  - Chạm nền vàng -> thu nhỏ.
    ///  - Kéo lá bài vào vùng XANH -> apply (hiện chữ + nháy sáng).
    /// Có thể xoá khi đã tích hợp vào game thật (Phase 3).
    /// </summary>
    public class CardInteractionDemo : MonoBehaviour
    {
        [SerializeField] private int cardCount = 5;

        private CardHandController _hand;
        private Image _applyImg;
        private Text _status;
        private Color _baseGreen = new(0.20f, 0.75f, 0.35f);
        private float _statusTimer;
        private float _flashTimer;

        private static readonly Color[] CardColors =
        {
            new(0.20f, 0.55f, 0.95f),
            new(0.85f, 0.35f, 0.75f),
            new(0.95f, 0.65f, 0.20f),
            new(0.30f, 0.80f, 0.70f),
            new(0.65f, 0.45f, 0.90f),
        };

        private void Start()
        {
            EnsureEventSystem();
            var canvas = BuildCanvas();
            _hand = canvas.gameObject.AddComponent<CardHandController>();

            // 🟨 Vùng trống (vàng) – chạm để thu nhỏ
            var bg = CreatePanel("EmptyArea_Yellow", canvas.transform,
                new Color(1f, 0.92f, 0.2f), Vector2.zero, Vector2.one, raycast: true);
            bg.gameObject.AddComponent<EmptyAreaClick>().hand = _hand;

            // 🟩 Vùng apply (xanh) – thả bài vào để kích hoạt
            _applyImg = CreatePanel("ApplyZone_Green", canvas.transform, _baseGreen,
                new Vector2(0.22f, 0.34f), new Vector2(0.78f, 0.96f), raycast: true);
            AddCenterLabel(_applyImg.transform, "APPLY ZONE", 44, new Color(1f, 1f, 1f, 0.5f));
            _hand.SetApplyZone((RectTransform)_applyImg.transform);

            // 🟥 Vùng hand (đỏ) – chỉ là nền trang trí, bài nằm phía trên
            CreatePanel("HandArea_Red", canvas.transform, new Color(0.85f, 0.2f, 0.2f),
                new Vector2(0.22f, 0.04f), new Vector2(0.78f, 0.30f), raycast: true);

            // Hướng dẫn + trạng thái
            _status = AddTopLabel(canvas.transform,
                "Tap card = zoom  |  Tap yellow = shrink  |  Drag card into GREEN = apply");

            // HandRoot: chứa các lá bài, nằm trên các panel, full màn hình để kéo tự do.
            var handRootGO = new GameObject("HandRoot", typeof(RectTransform));
            handRootGO.transform.SetParent(canvas.transform, false);
            var hr = (RectTransform)handRootGO.transform;
            hr.anchorMin = Vector2.zero;
            hr.anchorMax = Vector2.one;
            hr.offsetMin = Vector2.zero;
            hr.offsetMax = Vector2.zero;

            BuildCards(handRootGO.transform);

            _hand.OnCardApplied += HandleApplied;
        }

        private void Update()
        {
            if (_statusTimer > 0f)
            {
                _statusTimer -= Time.unscaledDeltaTime;
                if (_statusTimer <= 0f)
                    _status.text = "Tap card = zoom  |  Tap yellow = shrink  |  Drag card into GREEN = apply";
            }

            if (_flashTimer > 0f)
            {
                _flashTimer -= Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(_flashTimer / 0.4f);
                _applyImg.color = Color.Lerp(_baseGreen, Color.white, k);
            }
        }

        private void HandleApplied(CardInteraction card, Vector2 screenPos, Camera cam)
        {
            _status.text = $"Applied: {card.CardName}";
            _statusTimer = 1.5f;
            _flashTimer = 0.4f;
        }

        // ---------- Dựng UI ----------

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>(); // dùng Input System mới
        }

        private static Canvas BuildCanvas()
        {
            var go = new GameObject("CardCanvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static Image CreatePanel(string goName, Transform parent, Color color,
            Vector2 anchorMin, Vector2 anchorMax, bool raycast)
        {
            var go = new GameObject(goName, typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = raycast;
            return img;
        }

        private void BuildCards(Transform parent)
        {
            int n = Mathf.Max(1, cardCount);
            var size = new Vector2(190f, 270f);

            for (int i = 0; i < n; i++)
            {
                var color = CardColors[i % CardColors.Length];
                CreateCard(parent, size, color, (i + 1).ToString(), $"Card {i + 1}");
            }

            _hand.RelayoutHand(); // controller xếp quạt sau khi đã có đủ lá
        }

        private void CreateCard(Transform parent, Vector2 size, Color color,
            string label, string cardName)
        {
            var go = new GameObject($"Card_{cardName}",
                typeof(Image), typeof(CanvasGroup), typeof(CardInteraction));
            go.transform.SetParent(parent, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;

            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = true;

            AddCenterLabel(go.transform, label, 70, Color.white);

            var card = go.GetComponent<CardInteraction>();
            card.Setup(_hand, cardName);
            _hand.Register(card);
        }

        private static void AddCenterLabel(Transform parent, string text, int fontSize, Color color)
        {
            var go = new GameObject("Label", typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var txt = go.GetComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = text;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.raycastTarget = false;
        }

        private static Text AddTopLabel(Transform parent, string text)
        {
            var go = new GameObject("StatusLabel", typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -20f);
            rt.sizeDelta = new Vector2(0f, 60f);

            var txt = go.GetComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = text;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 30;
            txt.color = Color.white;
            txt.raycastTarget = false;
            return txt;
        }
    }
}
