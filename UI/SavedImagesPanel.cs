using UnityEngine;
using UnityEngine.UI;
using SlayTheTower.Drawing;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Panel hiển thị các ảnh ĐÃ LƯU (thumbnail trong Container có GridLayoutGroup).
    /// Bấm 1 ảnh → nạp vào khung vẽ (CardDrawingSurface) để vẽ tiếp.
    /// Nút mở panel → wire On Click → Open().
    /// </summary>
    public class SavedImagesPanel : MonoBehaviour
    {
        [Header("Tham chiếu")]
        [Tooltip("Panel để bật/tắt. Để trống = chính GameObject này.")]
        [SerializeField] private GameObject panel;
        [Tooltip("Container có GridLayoutGroup — nơi chứa thumbnail.")]
        [SerializeField] private Transform container;
        [Tooltip("Khung vẽ sẽ nhận ảnh khi bấm thumbnail.")]
        [SerializeField] private CardDrawingSurface surface;
        [Tooltip("Nút đóng (tùy chọn — tự nối vào Close()).")]
        [SerializeField] private Button closeButton;

        [SerializeField] private bool closeAfterPick = true;

        private System.Action<string> _pickHandler;
        private GameObject Panel => panel != null ? panel : gameObject;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        /// <summary>Mở panel (chế độ mặc định: chọn ảnh → nạp vào surface).</summary>
        public void Open()
        {
            _pickHandler = null;
            Panel.SetActive(true);
            Populate();
        }

        /// <summary>Mở panel để CHỌN 1 ảnh và trả id về callback (vd thêm vào dải frame),
        /// thay vì nạp vào surface.</summary>
        public void OpenForPick(System.Action<string> onPicked)
        {
            _pickHandler = onPicked;
            Panel.SetActive(true);
            Populate();
        }

        public void Close() => Panel.SetActive(false);

        public void Populate()
        {
            if (container == null) return;

            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);

            foreach (var id in CardArtStorage.ListSaved())
            {
                var sprite = CardArtStorage.Load(id);
                if (sprite == null) continue;

                var go = new GameObject(id, typeof(Image), typeof(Button));
                go.transform.SetParent(container, false);

                var img = go.GetComponent<Image>();
                img.sprite = sprite;
                img.preserveAspect = true;

                string captured = id; // tránh bug closure trong vòng lặp
                go.GetComponent<Button>().onClick.AddListener(() => Pick(captured));
            }
        }

        private void Pick(string id)
        {
            if (_pickHandler != null)
            {
                var handler = _pickHandler;
                _pickHandler = null;
                handler(id);
            }
            else if (surface != null)
            {
                surface.LoadFromSaved(id);
            }
            if (closeAfterPick) Close();
        }
    }
}
