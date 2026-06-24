using System;
using UnityEngine;

namespace SlayTheTower
{
    /// <summary>
    /// Máu dùng chung cho cả đơn vị quân và công trình (base).
    /// Phát sự kiện khi máu đổi / khi chết để UI và logic khác lắng nghe.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [SerializeField] private float maxHP = 100f;

        public float MaxHP => maxHP;
        public float CurrentHP { get; private set; }
        public bool IsDead { get; private set; }
        public float Normalized => maxHP > 0f ? CurrentHP / maxHP : 0f;

        /// <summary>(máu hiện tại, máu tối đa)</summary>
        public event Action<float, float> OnHealthChanged;
        /// <summary>Lượng sát thương vừa nhận (cho hiệu ứng trúng đòn / floating text).</summary>
        public event Action<float> OnDamaged;
        public event Action OnDied;

        private void Awake()
        {
            // Fallback nếu không ai gọi Setup (vd đặt sẵn trong scene).
            if (CurrentHP <= 0f && !IsDead)
                CurrentHP = maxHP;
        }

        /// <summary>Khởi tạo lại máu với giá trị tối đa mới (gọi khi spawn).</summary>
        public void Setup(float newMaxHP)
        {
            maxHP = Mathf.Max(1f, newMaxHP);
            CurrentHP = maxHP;
            IsDead = false;
            OnHealthChanged?.Invoke(CurrentHP, maxHP);
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f) return;

            CurrentHP = Mathf.Max(0f, CurrentHP - amount);
            OnDamaged?.Invoke(amount);
            OnHealthChanged?.Invoke(CurrentHP, maxHP);

            if (CurrentHP <= 0f)
            {
                IsDead = true;
                OnDied?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;

            CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
            OnHealthChanged?.Invoke(CurrentHP, maxHP);
        }
    }
}
