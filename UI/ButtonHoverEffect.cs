using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PianoGame
{
    /// <summary>
    /// Khi TRỎ CHUỘT vào (hover, chưa nhấn): đổi màu và/hoặc đổi ảnh của Image; rời chuột thì trả về như cũ.
    /// Bật 1 trong 2 (hoặc cả 2) trường hợp bằng <see cref="changeColor"/> / <see cref="changeSprite"/>.
    /// (Hover dùng cho PC/Editor — trên mobile cảm ứng KHÔNG có trạng thái hover.)
    /// </summary>
    public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Tooltip("Image bị tác động (để trống = Image trên chính object này).")]
        public Image targetImage;

        [Header("Trường hợp 1: Đổi màu")]
        [Tooltip("Bật đổi màu khi hover.")]
        public bool changeColor = true;
        [Tooltip("Màu khi đang trỏ chuột vào.")]
        public Color hoverColor = new Color(0.85f, 0.85f, 0.85f, 1f);

        [Header("Trường hợp 2: Đổi ảnh")]
        [Tooltip("Bật đổi ảnh (sprite) khi hover.")]
        public bool changeSprite = false;
        [Tooltip("Ảnh khi đang trỏ chuột vào.")]
        public Sprite hoverSprite;

        private Color _normalColor;
        private Sprite _normalSprite;

        private void Reset() => targetImage = GetComponent<Image>();

        private void Awake()
        {
            if (targetImage == null) targetImage = GetComponent<Image>();
            if (targetImage != null)
            {
                _normalColor = targetImage.color;
                _normalSprite = targetImage.sprite;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (targetImage == null) return;
            if (changeColor) targetImage.color = hoverColor;
            if (changeSprite && hoverSprite != null) targetImage.sprite = hoverSprite;
        }

        public void OnPointerExit(PointerEventData eventData) => Restore();

        private void OnDisable() => Restore();

        private void Restore()
        {
            if (targetImage == null) return;
            if (changeColor) targetImage.color = _normalColor;
            if (changeSprite) targetImage.sprite = _normalSprite;
        }
    }
}
