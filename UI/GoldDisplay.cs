using TMPro;
using UnityEngine;
using SlayTheTower.Data;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Hiển thị số vàng hiện có (TMP_Text), tự cập nhật khi <see cref="Wallet"/> thay đổi.
    /// Gắn lên 1 TextMeshPro - Text.
    /// </summary>
    public class GoldDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [Tooltip("Định dạng hiển thị; {0} = số vàng. Vd \"Vàng: {0}\".")]
        [SerializeField] private string format = "{0}";

        private void Reset() => label = GetComponent<TMP_Text>();

        private void OnEnable()
        {
            Wallet.OnGoldChanged += Set;
            Set(Wallet.Gold);
        }

        private void OnDisable() => Wallet.OnGoldChanged -= Set;

        private void Set(int gold)
        {
            if (label != null) label.text = string.Format(format, gold);
        }
    }
}
