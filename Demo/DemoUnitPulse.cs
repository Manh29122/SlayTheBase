using UnityEngine;

namespace SlayTheTower
{
    /// <summary>
    /// DEMO: phóng to giật nhẹ mỗi khi đơn vị tấn công — đứng thay cho animation thật.
    /// Trong game thật bạn dùng Animator (Unit tự gọi SetTrigger("Attack")).
    /// </summary>
    public class DemoUnitPulse : MonoBehaviour
    {
        private Vector3 _baseScale;
        private float _pulse;

        public void Init(Unit unit, Vector3 baseScale)
        {
            _baseScale = baseScale;
            if (unit != null) unit.OnAttack += () => _pulse = 1f;
        }

        private void Update()
        {
            if (_pulse <= 0f) return;
            _pulse = Mathf.Max(0f, _pulse - Time.deltaTime * 5f);
            transform.localScale = _baseScale * (1f + 0.45f * _pulse);
        }
    }
}
