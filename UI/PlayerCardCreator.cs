using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using SlayTheTower.Data;
using SlayTheTower.Drawing;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Tạo thẻ từ ảnh ĐÃ LƯU (không vẽ inline): chọn ảnh mặt thẻ qua picker, gõ tên/mô tả, đặt cost/loại,
    /// rồi lưu PlayerCardData. Hợp khi TÁCH RIÊNG màn vẽ và màn tạo card.
    /// </summary>
    public class PlayerCardCreator : MonoBehaviour
    {
        [Header("UI nguồn")]
        [Tooltip("Panel ảnh đã lưu, dùng để chọn ảnh mặt thẻ.")]
        [SerializeField] private SavedImagesPanel imagePicker;
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_InputField descriptionInput;
        [Tooltip("(Tùy chọn) Image hiển thị ảnh mặt thẻ đang chọn.")]
        [SerializeField] private Image artPreview;

        [Header("Thuộc tính thẻ")]
        [SerializeField] private int energyCost = 3;
        [Tooltip("Giá mua/bán trong shop (vàng).")]
        [SerializeField] private int shopPrice = 50;
        [SerializeField] private CardType cardType = CardType.SummonUnit;
        [Tooltip("Khi cardType = SummonUnit: id UnitDefinition sẽ triệu hồi.")]
        [SerializeField] private string unitId;
        [Tooltip("Id ảnh nền (nếu có chọn nền từ BackgroundLibrary).")]
        [SerializeField] private string backgroundId;
        [Tooltip("Id ảnh mặt thẻ (ảnh đã lưu được chọn).")]
        [SerializeField] private string cardArtId;

        [Header("Sau khi tạo")]
        [SerializeField] private bool clearAfterCreate = true;
        [Tooltip("Bắn ra id thẻ vừa tạo (vd để refresh Collection).")]
        public UnityEvent<string> onCardCreated;

        // ----- Chọn ảnh mặt thẻ (mở picker) -----
        public void PickCardArt() => imagePicker.OpenForPick(SetCardArt);

        public void SetCardArt(string id)
        {
            cardArtId = id;
            if (artPreview != null) artPreview.sprite = CardArtStorage.Load(id);
        }

        // ----- Setter để nối UI (Inspector) -----
        public void SetCost(int c) => energyCost = Mathf.Max(0, c);
        public void SetCostNormalized(float t) => energyCost = Mathf.RoundToInt(Mathf.Lerp(0f, 10f, Mathf.Clamp01(t)));
        public void SetShopPrice(int p) => shopPrice = Mathf.Max(0, p);
        public void SetCardType(int typeIndex) => cardType = (CardType)typeIndex; // 0 = SummonUnit, 1 = Buff
        public void SetUnitId(string id) => unitId = id;
        public void SetBackgroundId(string id) => backgroundId = id;

        /// <summary>Tạo PlayerCardData từ ảnh đã chọn + thông tin, rồi ghi JSON. Nối vào nút "Tạo thẻ".</summary>
        public void CreateCard()
        {
            string id = "card_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + Guid.NewGuid().ToString("N")[..6];
            var data = new PlayerCardData
            {
                id = id,
                displayName = nameInput != null ? nameInput.text : "",
                description = descriptionInput != null ? descriptionInput.text : "",
                energyCost = energyCost,
                shopPrice = shopPrice,
                cardType = cardType,
                unitId = unitId,
                backgroundId = backgroundId,
                artFile = cardArtId // ảnh đã lưu được chọn (không vẽ inline)
            };
            PlayerCardStorage.Add(data);
            onCardCreated?.Invoke(id);
            Debug.Log($"[PlayerCardCreator] Đã tạo thẻ '{data.displayName}' (id {id}, art {cardArtId}).");

            if (clearAfterCreate)
            {
                if (nameInput != null) nameInput.text = "";
                if (descriptionInput != null) descriptionInput.text = "";
                cardArtId = "";
                if (artPreview != null) artPreview.sprite = null;
            }
        }
    }
}
