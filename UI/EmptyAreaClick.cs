using UnityEngine;
using UnityEngine.EventSystems;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Gắn lên panel "vùng trống" (màu vàng). Chạm vào đây sẽ bỏ chọn (thu nhỏ) lá bài đang chọn.
    /// </summary>
    public class EmptyAreaClick : MonoBehaviour, IPointerClickHandler
    {
        public CardHandController hand;

        public void OnPointerClick(PointerEventData e)
        {
            if (hand != null) hand.DeselectAll();
        }
    }
}
