using System.Collections.Generic;
using UnityEngine;

namespace SlayTheTower
{
    /// <summary>
    /// Vùng gây sát thương theo thời gian (DoT) cho một phe — độc/cháy.
    /// Mỗi nhịp (tick) gây sát thương cho các đơn vị thuộc phe bị ảnh hưởng nằm trong bán kính.
    /// </summary>
    public class DamageZone : MonoBehaviour
    {
        private Team _affectedTeam;
        private float _radius;
        private float _damagePerTick;
        private float _tickInterval;
        private float _life;
        private float _tickTimer;
        private readonly List<Unit> _buf = new();

        public void Init(Team affectedTeam, float radius, float damagePerSecond, float tickInterval, float life)
        {
            _affectedTeam = affectedTeam;
            _radius = radius;
            _tickInterval = Mathf.Max(0.1f, tickInterval);
            _damagePerTick = damagePerSecond * _tickInterval;
            _life = life;
            _tickTimer = 0f; // tick ngay lần đầu
        }

        private void Update()
        {
            _life -= Time.deltaTime;
            _tickTimer -= Time.deltaTime;
            if (_tickTimer <= 0f)
            {
                _tickTimer = _tickInterval;
                UnitRegistry.FindUnitsOfTeamInArea(_affectedTeam, transform.position, _radius, _buf);
                for (int i = 0; i < _buf.Count; i++)
                    _buf[i].Health.TakeDamage(_damagePerTick);
            }
            if (_life <= 0f) Destroy(gameObject);
        }
    }
}
