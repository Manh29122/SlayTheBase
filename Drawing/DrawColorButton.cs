using UnityEngine;
using UnityEngine.UI;

namespace SlayTheTower.Drawing
{
    /// <summary>
    /// Nút chọn MÀU cho khung vẽ. Gắn lên 1 Button (ô màu): bấm sẽ đặt màu cọ cho CardDrawingSurface.
    /// Dùng vì Button.onClick trong Inspector không điền được kiểu Color.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class DrawColorButton : MonoBehaviour
    {
        [SerializeField] private CardDrawingSurface surface;
        [SerializeField] private Color color = Color.black;
        [Tooltip("Tự tô màu này lên Image của nút để thấy được màu.")]
        [SerializeField] private bool tintThisButton = true;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Apply);
            if (tintThisButton)
            {
                var img = GetComponent<Image>();
                if (img != null) img.color = color;
            }
        }

        public void Apply()
        {
            if (surface != null) surface.SetBrushColor(color);
        }
    }
}
