using UnityEngine;
using UnityEngine.EventSystems;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Cho phép KÉO 1 lá bài trong màn tạo deck. Khi thả vào một <see cref="DeckDropZone"/>,
    /// zone đó sẽ nhận lá (đổi cha). Nếu thả ra ngoài → lá tự về chỗ cũ.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class DeckCardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private RectTransform _rect;
        private CanvasGroup _cg;
        private Canvas _canvas;
        private Transform _origin;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            _cg = GetComponent<CanvasGroup>();
            _canvas = GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData e)
        {
            _origin = transform.parent;
            if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
            transform.SetParent(_canvas.transform, true); // tách khỏi LayoutGroup để kéo tự do
            transform.SetAsLastSibling();                 // nổi lên trên
            _cg.blocksRaycasts = false;                   // để zone bên dưới nhận được drop
        }

        public void OnDrag(PointerEventData e)
        {
            float sf = _canvas != null ? _canvas.scaleFactor : 1f;
            _rect.anchoredPosition += e.delta / sf;
        }

        public void OnEndDrag(PointerEventData e)
        {
            _cg.blocksRaycasts = true;
            // Không zone nào nhận (vẫn nằm dưới canvas) -> trả về vùng cũ.
            if (transform.parent == _canvas.transform)
                transform.SetParent(_origin, false);
        }
    }
}
