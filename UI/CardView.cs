using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SlayTheTower.Data;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Gắn lên PREFAB lá bài (do bạn tự thiết kế). Giữ dữ liệu gameplay (<see cref="CardDefinition"/>)
    /// và (tùy chọn) tự đổ text/ảnh/màu vào các UI con khi <see cref="Refresh"/> được gọi.
    /// Trang trí riêng của từng lá cứ để trên prefab — chỉ gán ref nào bạn muốn bind tự động.
    /// </summary>
    [RequireComponent(typeof(CardInteraction))]
    public class CardView : MonoBehaviour
    {
        [Tooltip("Dữ liệu lá bài (cost, loại, triệu hồi gì / buff gì...). Dùng cho gameplay.")]
        [SerializeField] private CardDefinition definition;

        [Header("Tham chiếu UI (tùy chọn — để trống nếu trang trí hoàn toàn thủ công)")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image artworkImage;
        [SerializeField] private Text nameText;
        [SerializeField] private Text costText;
        [SerializeField] private Text descriptionText;
        [Tooltip("Giá mua/bán (vàng) trong shop.")]
        [SerializeField] private Text priceText;
        [Tooltip("Số lá đang sở hữu (vd 'x3').")]
        [SerializeField] private Text ownedText;

        [Header("Hoạt ảnh artwork (kéo các sprite vào để chạy)")]
        [Tooltip("Danh sách sprite cho hoạt ảnh artwork. Để TRỐNG = dùng artwork tĩnh của definition.")]
        [SerializeField] private List<Sprite> artFrames = new();
        [Tooltip("Thời gian mỗi frame (giây).")]
        [Min(0.01f)] [SerializeField] private float artFrameDuration = 0.15f;
        [Tooltip("Lặp lại hoạt ảnh.")]
        [SerializeField] private bool loopArt = true;

        private int _artIndex;
        private float _artTimer;

        public CardDefinition Definition => definition;
        public string DisplayName => definition != null ? definition.displayName : name;

        private void Reset()
        {
            // Tự bắt Image ở root làm background cho tiện khi mới Add Component.
            backgroundImage = GetComponent<Image>();
        }

        private void Awake() => Refresh();

        private void OnEnable()
        {
            CardInventory.OnChanged += RefreshOwned;
            _artIndex = 0;
            _artTimer = 0f;
            ApplyArtFrame();
        }

        private void OnDisable() => CardInventory.OnChanged -= RefreshOwned;

        private void Update()
        {
            if (artworkImage == null || artFrames == null || artFrames.Count == 0 || artFrameDuration <= 0f)
                return;
            if (!loopArt && _artIndex >= artFrames.Count - 1) return; // hết & không lặp -> dừng

            _artTimer += Time.deltaTime;
            if (_artTimer < artFrameDuration) return;
            _artTimer -= artFrameDuration;

            _artIndex++;
            if (_artIndex >= artFrames.Count) _artIndex = loopArt ? 0 : artFrames.Count - 1;
            ApplyArtFrame();
        }

        private void ApplyArtFrame()
        {
            if (artworkImage == null || artFrames == null || artFrames.Count == 0) return;
            int idx = Mathf.Clamp(_artIndex, 0, artFrames.Count - 1);
            if (artFrames[idx] != null) artworkImage.sprite = artFrames[idx];
        }

        /// <summary>Gán list frame artwork bằng code (vd thẻ người chơi animated).</summary>
        public void SetArtFrames(List<Sprite> frames, float frameDuration = -1f, bool loop = true)
        {
            artFrames = frames ?? new List<Sprite>();
            if (frameDuration > 0f) artFrameDuration = frameDuration;
            loopArt = loop;
            _artIndex = 0;
            _artTimer = 0f;
            ApplyArtFrame();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying) Refresh(); // xem trước ngay trong lúc chỉnh prefab
        }
#endif

        /// <summary>Gán data bằng code (vd khi tái dùng 1 prefab mẫu cho nhiều lá).</summary>
        public void SetDefinition(CardDefinition def)
        {
            definition = def;
            Refresh();
        }

        /// <summary>Gán nhanh các tham chiếu UI bằng code (dùng khi dựng prefab runtime/demo).</summary>
        public void BindUI(Image background, Image artwork, Text nameLabel, Text costLabel, Text descLabel)
        {
            backgroundImage = background;
            artworkImage = artwork;
            nameText = nameLabel;
            costText = costLabel;
            descriptionText = descLabel;
        }

        /// <summary>Ghi đè số cost hiển thị (vd để hiện chi phí hiệu lực sau khi giảm giá/nâng cấp).</summary>
        public void SetDisplayedCost(int cost)
        {
            if (costText != null) costText.text = cost.ToString();
        }

        /// <summary>Đổ dữ liệu từ definition vào các UI đã gán.</summary>
        public void Refresh()
        {
            if (definition == null) return;

            // Chỉ đổ NỘI DUNG (text + artwork). Nền/khung/màu chữ do prefab tự chỉnh, KHÔNG ghi đè.
            if (nameText != null) nameText.text = definition.displayName;
            if (costText != null) costText.text = definition.energyCost.ToString();
            if (descriptionText != null) descriptionText.text = definition.description;
            if (artworkImage != null)
            {
                if (artFrames != null && artFrames.Count > 0 && artFrames[0] != null)
                    artworkImage.sprite = artFrames[0];       // có frame -> dùng frame đầu
                else if (definition.artwork != null)
                    artworkImage.sprite = definition.artwork; // không -> artwork tĩnh
            }
            if (priceText != null) priceText.text = definition.shopPrice.ToString();
            RefreshOwned();
        }

        /// <summary>Cập nhật riêng số lá sở hữu (gọi khi kho thay đổi).</summary>
        private void RefreshOwned()
        {
            if (ownedText != null && definition != null)
                ownedText.text = "x" + CardInventory.GetCount(definition.id);
        }
    }
}
