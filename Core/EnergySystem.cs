using System;
using UnityEngine;

namespace SlayTheTower
{
    /// <summary>
    /// Năng lượng (mana) hồi theo thời gian. Đánh thẻ tốn energy; đơn vị/buff hỗ trợ
    /// làm tăng tốc độ hồi qua <see cref="AddRegen"/>.
    /// </summary>
    public class EnergySystem : MonoBehaviour
    {
        [SerializeField] private float maxEnergy = 10f;
        [SerializeField] private float startEnergy = 5f;
        [Tooltip("Lượng energy hồi mỗi giây.")]
        [SerializeField] private float regenPerSecond = 1f;

        public float Current { get; private set; }
        public float Max => maxEnergy;
        public float RegenPerSecond => regenPerSecond;

        /// <summary>(hiện tại, tối đa)</summary>
        public event Action<float, float> OnEnergyChanged;

        private void Awake()
        {
            Current = Mathf.Clamp(startEnergy, 0f, maxEnergy);
        }

        private void Start() => OnEnergyChanged?.Invoke(Current, maxEnergy);

        private void Update()
        {
            if (Current < maxEnergy)
            {
                Current = Mathf.Min(maxEnergy, Current + regenPerSecond * Time.deltaTime);
                OnEnergyChanged?.Invoke(Current, maxEnergy);
            }
        }

        public bool CanAfford(float cost) => Current >= cost;

        public bool TrySpend(float cost)
        {
            if (Current < cost) return false;
            Current = Mathf.Max(0f, Current - cost);
            OnEnergyChanged?.Invoke(Current, maxEnergy);
            return true;
        }

        /// <summary>Cộng/trừ tốc độ hồi (dùng cho buff mana).</summary>
        public void AddRegen(float delta)
        {
            regenPerSecond = Mathf.Max(0f, regenPerSecond + delta);
        }

        /// <summary>Cộng energy NGAY lập tức (Energy Burst).</summary>
        public void AddEnergyNow(float amount)
        {
            Current = Mathf.Clamp(Current + amount, 0f, maxEnergy);
            OnEnergyChanged?.Invoke(Current, maxEnergy);
        }

        /// <summary>Nâng trần energy tối đa.</summary>
        public void AddMaxEnergy(float amount)
        {
            maxEnergy = Mathf.Max(1f, maxEnergy + amount);
            Current = Mathf.Min(Current, maxEnergy);
            OnEnergyChanged?.Invoke(Current, maxEnergy);
        }
    }
}
