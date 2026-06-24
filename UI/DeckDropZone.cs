using UnityEngine;
using UnityEngine.EventSystems;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Vùng nhận lá khi kéo-thả (gắn lên CẢ container trái lẫn phải).
    /// Cần 1 Graphic (Image) bật Raycast Target để nhận được drop.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class DeckDropZone : MonoBehaviour, IDropHandler
    {
        public void OnDrop(PointerEventData e)
        {
            var go = e.pointerDrag;
            if (go == null) return;
            if (go.GetComponent<DeckCardDrag>() == null) return;

            go.transform.SetParent(transform, false); // nhận lá vào vùng này (LayoutGroup tự xếp)
        }
    }
}
