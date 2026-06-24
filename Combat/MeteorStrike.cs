using System.Collections.Generic;
using UnityEngine;

namespace SlayTheTower
{
    /// <summary>
    /// Thiên thạch: báo trước (telegraph) trong <c>delay</c> giây rồi giáng sát thương AoE một lần
    /// xuống mọi đơn vị thuộc phe bị ảnh hưởng trong bán kính. Địch có thể né nếu rời khỏi vùng kịp.
    /// Nếu GameObject có SpriteRenderer thì nhấp nháy alpha làm hiệu ứng cảnh báo.
    /// </summary>
    public class MeteorStrike : MonoBehaviour
    {
        private Team _affectedTeam;
        private float _radius;
        private float _damage;
        private float _delay;
        private float _timer;
        private SpriteRenderer _sr;
        private Color _baseColor;
        private readonly List<Unit> _buf = new();

        public void Init(Team affectedTeam, float radius, float damage, float delay)
        {
            _affectedTeam = affectedTeam;
            _radius = radius;
            _damage = damage;
            _delay = Mathf.Max(0.05f, delay);
            _timer = _delay;
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null) _baseColor = _sr.color;
        }

        private void Update()
        {
            _timer -= Time.deltaTime;

            if (_sr != null)
            {
                float urgency = 1f - Mathf.Clamp01(_timer / _delay); // 0 -> 1 khi gần giáng
                float blink = Mathf.PingPong(Time.time * (3f + 6f * urgency), 1f);
                float a = Mathf.Lerp(0.15f, 0.6f, blink);
                _sr.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, a);
            }

            if (_timer <= 0f)
            {
                UnitRegistry.FindUnitsOfTeamInArea(_affectedTeam, transform.position, _radius, _buf);
                for (int i = 0; i < _buf.Count; i++)
                    _buf[i].Health.TakeDamage(_damage);
                Destroy(gameObject);
            }
        }
    }
}
