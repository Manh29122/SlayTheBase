using System;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using SlayTheTower.Data;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Tạo UNIT CARD: chọn frame Walk + Attack (theo thứ tự) qua picker, chỉnh stats & thời gian,
    /// rồi lưu PlayerUnitData + một PlayerCardData (thẻ triệu hồi unit đó).
    /// Wire các nút/ô nhập vào những hàm public dưới đây.
    /// </summary>
    public class UnitCardCreator : MonoBehaviour
    {
        [Header("UI nguồn")]
        [SerializeField] private SavedImagesPanel imagePicker;      // panel ảnh đã lưu
        [SerializeField] private FrameSequenceBuilder walkBuilder;  // dải frame Walk
        [SerializeField] private FrameSequenceBuilder attackBuilder;// dải frame Attack
        [SerializeField] private TMP_InputField nameInput;

        [Header("Thẻ")]
        [SerializeField] private int energyCost = 3;
        [Tooltip("Giá mua/bán trong shop (vàng).")]
        [SerializeField] private int shopPrice = 50;
        [Tooltip("Id ảnh mặt thẻ (để trống = dùng frame Walk đầu tiên).")]
        [SerializeField] private string cardArtId;

        [Header("Chỉ số unit")]
        [SerializeField] private float maxHP = 100f;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private AttackType attackType = AttackType.Melee;
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private float attackRange = 1.5f;

        [Header("Thời gian hoạt ảnh")]
        [SerializeField] private float walkDuration = 0.6f;
        [SerializeField] private float attackDuration = 0.4f;
        [SerializeField] private int attackHitFrame = 1;
        [SerializeField] private float attackCooldown = 0.5f;

        [Header("Đạn (tầm xa)")]
        [SerializeField] private string projectileFrameId;
        [SerializeField] private float projectileSpeed = 8f;

        [Tooltip("Bắn ra id unit/thẻ vừa tạo.")]
        public UnityEvent<string> onUnitCreated;

        // ----- Nút chọn frame (mở picker, nối id vào dải tương ứng) -----
        public void PickWalkFrame() => imagePicker.OpenForPick(id => walkBuilder.AddFrame(id));
        public void PickAttackFrame() => imagePicker.OpenForPick(id => attackBuilder.AddFrame(id));
        public void PickProjectileImage() => imagePicker.OpenForPick(id => projectileFrameId = id);
        public void PickCardArt() => imagePicker.OpenForPick(id => cardArtId = id);

        public void RemoveLastWalkFrame() => walkBuilder.RemoveLast();
        public void RemoveLastAttackFrame() => attackBuilder.RemoveLast();

        // ----- Setter để nối Slider/Scrollbar/Dropdown (Inspector) -----
        public void SetCost(int v) => energyCost = Mathf.Max(0, v);
        public void SetShopPrice(int v) => shopPrice = Mathf.Max(0, v);
        public void SetHP(float v) => maxHP = Mathf.Max(1f, v);
        public void SetMoveSpeed(float v) => moveSpeed = Mathf.Max(0f, v);
        public void SetDamage(float v) => attackDamage = Mathf.Max(0f, v);
        public void SetRange(float v) => attackRange = Mathf.Max(0f, v);
        public void SetAttackType(int idx) => attackType = (AttackType)idx; // 0 = Melee, 1 = Ranged
        public void SetWalkDuration(float v) => walkDuration = Mathf.Max(0.05f, v);
        public void SetAttackDuration(float v) => attackDuration = Mathf.Max(0.05f, v);
        public void SetAttackHitFrame(int v) => attackHitFrame = Mathf.Max(0, v);
        public void SetAttackCooldown(float v) => attackCooldown = Mathf.Max(0f, v);
        public void SetProjectileSpeed(float v) => projectileSpeed = Mathf.Max(0.1f, v);

        /// <summary>Lưu unit + thẻ triệu hồi unit đó. Nối vào nút "Tạo Unit Card".</summary>
        public void CreateUnitCard()
        {
            string id = "unit_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + Guid.NewGuid().ToString("N")[..6];
            string displayName = nameInput != null ? nameInput.text : "";

            var unit = new PlayerUnitData
            {
                id = id,
                displayName = displayName,
                maxHP = maxHP,
                moveSpeed = moveSpeed,
                attackType = attackType,
                attackDamage = attackDamage,
                attackRange = attackRange,
                attackCooldown = attackCooldown,
                walkFrameIds = walkBuilder != null ? walkBuilder.ToArray() : new string[0],
                walkDuration = walkDuration,
                attackFrameIds = attackBuilder != null ? attackBuilder.ToArray() : new string[0],
                attackDuration = attackDuration,
                attackHitFrame = attackHitFrame,
                projectileFrameId = projectileFrameId,
                projectileSpeed = projectileSpeed
            };
            PlayerUnitStorage.Add(unit);

            // Thẻ triệu hồi unit này
            string artId = !string.IsNullOrEmpty(cardArtId) ? cardArtId
                : (unit.walkFrameIds.Length > 0 ? unit.walkFrameIds[0] : "");
            var card = new PlayerCardData
            {
                id = id,
                displayName = displayName,
                energyCost = energyCost,
                shopPrice = shopPrice,
                cardType = CardType.SummonUnit,
                unitId = id,
                artFile = artId
            };
            PlayerCardStorage.Add(card);

            onUnitCreated?.Invoke(id);
            Debug.Log($"[UnitCardCreator] Đã tạo unit card '{displayName}' (id {id}).");
        }
    }
}
