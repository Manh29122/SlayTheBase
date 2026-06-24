using System.Collections.Generic;
using UnityEngine;

namespace SlayTheTower
{
    /// <summary>
    /// Sổ đăng ký toàn bộ đơn vị đang sống để tìm mục tiêu nhanh (không cần collider).
    /// Đơn vị tự đăng ký/hủy đăng ký khi enable/disable.
    /// </summary>
    public static class UnitRegistry
    {
        private static readonly List<Unit> _units = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay() => _units.Clear();

        public static void Register(Unit u)
        {
            if (u != null && !_units.Contains(u)) _units.Add(u);
        }

        public static void Unregister(Unit u) => _units.Remove(u);

        /// <summary>Lấy mọi đơn vị thuộc <paramref name="team"/> trong bán kính (ghi vào buffer, không cấp phát mới).</summary>
        public static void FindUnitsOfTeamInArea(Team team, Vector3 pos, float radius, List<Unit> buffer)
        {
            buffer.Clear();
            float rSq = radius * radius;
            for (int i = 0; i < _units.Count; i++)
            {
                var u = _units[i];
                if (u == null || u.IsDead || u.Team != team) continue;
                if (((Vector2)(u.transform.position - pos)).sqrMagnitude <= rSq)
                    buffer.Add(u);
            }
        }

        /// <summary>Tìm đơn vị PHE ĐỊCH gần nhất trong bán kính cho trước (null nếu không có).</summary>
        public static Unit FindNearestEnemy(Team myTeam, Vector3 pos, float maxRange)
        {
            Unit best = null;
            float bestSq = maxRange * maxRange;
            for (int i = 0; i < _units.Count; i++)
            {
                var u = _units[i];
                if (u == null || u.IsDead || u.Team == myTeam) continue;
                float dSq = ((Vector2)(u.transform.position - pos)).sqrMagnitude;
                if (dSq <= bestSq) { bestSq = dSq; best = u; }
            }
            return best;
        }
    }
}
