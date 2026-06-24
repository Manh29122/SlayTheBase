using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using SlayTheTower.Data;
using SlayTheTower.UI;

namespace SlayTheTower
{
    /// <summary>
    /// BOOTSTRAP DEMO Phase 3 — Energy + Deck + Hand (deck-driven) + Buff.
    /// Tự dựng mọi thứ bằng code: bấm Play để test.
    ///  - Thanh energy hồi theo thời gian.
    ///  - Tay bài rút từ deck (cycling), kéo lá vào vùng XANH để đánh (tốn energy).
    ///  - Các buff deck/energy chạy thật: Draw +2, Mana +1, Recycle, Hand +/-1, Cost x0.5...
    /// Buff lên quân / zone hiện chỉ log (chờ Phase 2 buff-runtime + Phase 4 zone).
    /// </summary>
    public class Phase3Demo : MonoBehaviour
    {
        private EnergySystem _energy;
        private BattleDeck _deck;
        private HandView _hand;
        private GameObject _cardTemplate;

        private Image _energyFill;
        private Text _energyText;
        private Text _debugText;

        private void Awake()
        {
            EnsureEventSystem();
            var canvas = BuildCanvas();

            // Vùng apply (xanh) + hand root + HUD
            var applyZone = CreatePanel("ApplyZone_Green", canvas.transform,
                new Color(0.20f, 0.75f, 0.35f), new Vector2(0.20f, 0.34f), new Vector2(0.80f, 0.95f), true);
            AddCenterLabel(applyZone.transform, "APPLY ZONE\n(kéo thẻ vào đây để đánh)", 34,
                new Color(1f, 1f, 1f, 0.6f));

            var handRoot = CreateFullRect("HandRoot", canvas.transform);
            BuildEnergyBar(canvas.transform);
            _debugText = AddCornerLabel(canvas.transform, "", new Vector2(0f, 1f), new Vector2(20f, -20f), 26);
            AddTopLabel(canvas.transform, "Tap = zoom | Tap lại = thu nhỏ | Drag vào GREEN = đánh thẻ (tốn energy)");

            _cardTemplate = BuildCardTemplate(canvas.transform);

            // Hệ thống
            _energy = gameObject.AddComponent<EnergySystem>();
            _deck = gameObject.AddComponent<BattleDeck>();
            var ctx = gameObject.AddComponent<BattleContext>();

            var handGO = new GameObject("Hand", typeof(CardHandController), typeof(HandView));
            handGO.transform.SetParent(canvas.transform, false);
            _hand = handGO.GetComponent<HandView>();

            ctx.Configure(_energy, _deck, _hand, null, null, null);
            _hand.Configure(_energy, _deck, ctx, (RectTransform)handRoot.transform,
                (RectTransform)applyZone.transform, _cardTemplate);

            _deck.Initialize(BuildDeck(), handLimit: 4);

            _energy.OnEnergyChanged += UpdateEnergyBar;
            _hand.BuildHand();
            UpdateEnergyBar(_energy.Current, _energy.Max);
        }

        private void Update()
        {
            if (_debugText != null && _deck != null && _hand != null)
                _debugText.text =
                    $"Hand: {_hand.Hand.Count}/{_deck.HandLimit}   Deck: {_deck.DrawPileCount}   " +
                    $"CostX: {_deck.CostMultiplier:0.##}   Free: {_deck.FreeCharges}   Regen: {_energy.RegenPerSecond:0.#}/s";
        }

        // ---------- Bộ bài demo ----------
        private List<CardDefinition> BuildDeck()
        {
            var list = new List<CardDefinition>();

            void Add(CardDefinition c, int n) { for (int i = 0; i < n; i++) list.Add(c); }

            Add(MakeSummon("Warrior", 3, new Color(0.55f, 0.35f, 0.25f)), 2);
            Add(MakeSummon("Archer", 2, new Color(0.30f, 0.55f, 0.30f)), 2);

            Add(MakeBuff("Draw +2", 2, new Color(0.25f, 0.45f, 0.85f),
                MakeEffect(CardEffectType.DrawCards, count: 2)), 1);
            Add(MakeBuff("Mana +1", 3, new Color(0.30f, 0.65f, 0.85f),
                MakeEffect(CardEffectType.ManaRegenAdd, amount: 1f)), 1);
            Add(MakeBuff("Recycle 5", 1, new Color(0.60f, 0.45f, 0.85f),
                MakeEffect(CardEffectType.RecycleHandDrawTop, count: 5)), 1);
            Add(MakeBuff("Hand +1", 4, new Color(0.85f, 0.60f, 0.25f),
                MakeEffect(CardEffectType.HandLimitDelta, handLimitDelta: 1)), 1);
            Add(MakeBuff("Hand -1", 0, new Color(0.85f, 0.45f, 0.45f),
                MakeEffect(CardEffectType.HandLimitDelta, handLimitDelta: -1)), 1);
            Add(MakeBuff("Cost x0.5", 1, new Color(0.45f, 0.75f, 0.65f),
                MakeEffect(CardEffectType.EnergyCostMultiplier, amount: 0.5f)), 1);

            Add(MakeBuff("Energy +5", 0, new Color(0.30f, 0.85f, 0.95f),
                MakeEffect(CardEffectType.EnergyBurst, amount: 5f)), 1);
            Add(MakeBuff("Max +3", 4, new Color(0.25f, 0.55f, 0.80f),
                MakeEffect(CardEffectType.MaxEnergyAdd, amount: 3f)), 1);
            Add(MakeBuff("Next Free", 2, new Color(0.75f, 0.85f, 0.35f),
                MakeEffect(CardEffectType.NextCardFree, count: 1)), 1);
            Add(MakeBuff("Upgrade", 3, new Color(0.85f, 0.75f, 0.35f),
                MakeEffect(CardEffectType.UpgradeRandomCard, count: 1)), 1);

            return list;
        }

        private CardDefinition MakeSummon(string name, int cost, Color color)
        {
            var c = ScriptableObject.CreateInstance<CardDefinition>();
            c.displayName = name; c.energyCost = cost;
            c.cardType = CardType.SummonUnit; c.cardPrefab = _cardTemplate;
            return c;
        }

        private CardDefinition MakeBuff(string name, int cost, Color color, CardEffectDefinition effect)
        {
            var c = ScriptableObject.CreateInstance<CardDefinition>();
            c.displayName = name; c.energyCost = cost;
            c.cardType = CardType.Buff; c.cardPrefab = _cardTemplate;
            c.effects = new List<CardEffectDefinition> { effect };
            return c;
        }

        private static CardEffectDefinition MakeEffect(CardEffectType type,
            float amount = 0f, int count = 1, int handLimitDelta = 0)
        {
            var e = ScriptableObject.CreateInstance<CardEffectDefinition>();
            e.effectType = type;
            e.amount = amount;
            e.count = count;
            e.secondaryCount = count;
            e.handLimitDelta = handLimitDelta;
            e.handLimitMin = 3; e.handLimitMax = 7;
            return e;
        }

        // ---------- Dựng UI ----------
        private GameObject BuildCardTemplate(Transform parent)
        {
            var go = new GameObject("CardTemplate",
                typeof(Image), typeof(CanvasGroup), typeof(CardInteraction), typeof(CardView));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(180f, 250f);

            var bg = go.GetComponent<Image>();
            bg.color = Color.white;

            var nameText = AddCenterLabel(go.transform, "", 40, Color.white);
            var costText = AddCornerLabel(go.transform, "", new Vector2(0f, 1f), new Vector2(14f, -10f), 44);
            costText.color = Color.yellow;

            go.GetComponent<CardView>().BindUI(bg, null, nameText, costText, null);
            go.SetActive(false); // chỉ làm khuôn, không hiển thị
            return go;
        }

        private void BuildEnergyBar(Transform parent)
        {
            var container = CreatePanel("EnergyBar", parent, new Color(0f, 0f, 0f, 0.6f),
                new Vector2(0.02f, 0.02f), new Vector2(0.30f, 0.07f), false);

            _energyFill = CreatePanel("Fill", container.transform, new Color(0.25f, 0.7f, 1f),
                new Vector2(0f, 0f), new Vector2(1f, 1f), false);

            _energyText = AddCenterLabel(container.transform, "", 28, Color.white);
        }

        private void UpdateEnergyBar(float current, float max)
        {
            if (_energyFill != null)
            {
                var rt = (RectTransform)_energyFill.transform;
                rt.anchorMax = new Vector2(max > 0 ? current / max : 0f, 1f);
            }
            if (_energyText != null) _energyText.text = $"Energy: {current:0.0} / {max:0}";
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
            var go = new GameObject("BattleCanvas",
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

        private static Image CreatePanel(string n, Transform parent, Color color,
            Vector2 anchorMin, Vector2 anchorMax, bool raycast)
        {
            var go = new GameObject(n, typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = color; img.raycastTarget = raycast;
            return img;
        }

        private static GameObject CreateFullRect(string n, Transform parent)
        {
            var go = new GameObject(n, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return go;
        }

        private static Text AddCenterLabel(Transform parent, string text, int fontSize, Color color)
        {
            var go = new GameObject("Label", typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var txt = go.GetComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = text; txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = fontSize; txt.color = color; txt.raycastTarget = false;
            return txt;
        }

        private static Text AddCornerLabel(Transform parent, string text,
            Vector2 anchor, Vector2 offset, int fontSize)
        {
            var go = new GameObject("Corner", typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = offset;
            rt.sizeDelta = new Vector2(600f, 50f);
            var txt = go.GetComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = text;
            txt.alignment = anchor.x < 0.5f ? TextAnchor.UpperLeft : TextAnchor.UpperRight;
            txt.fontSize = fontSize; txt.color = Color.white; txt.raycastTarget = false;
            return txt;
        }

        private static void AddTopLabel(Transform parent, string text)
        {
            var go = new GameObject("TopLabel", typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -70f);
            rt.sizeDelta = new Vector2(0f, 50f);
            var txt = go.GetComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = text; txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 26; txt.color = new Color(1f, 1f, 1f, 0.8f); txt.raycastTarget = false;
        }
    }
}
