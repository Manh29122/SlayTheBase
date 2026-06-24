using System.Collections.Generic;
using UnityEngine;

namespace SlayTheTower
{
    /// <summary>
    /// Tuyến đường cố định nối base người chơi (waypoint đầu) và base địch (waypoint cuối).
    /// Quân Player đi từ waypoint[0] -> waypoint[cuối]; quân Enemy đi ngược lại.
    /// </summary>
    public class PathSystem : MonoBehaviour
    {
        [Tooltip("Các điểm mốc theo thứ tự: [0] phía base Player, [cuối] phía base Enemy.")]
        [SerializeField] private List<Transform> waypoints = new List<Transform>();

        public int Count => waypoints.Count;

        public Vector3 GetWaypoint(int index)
        {
            if (waypoints.Count == 0) return transform.position;
            index = Mathf.Clamp(index, 0, waypoints.Count - 1);
            return waypoints[index].position;
        }

        /// <summary>Thay toàn bộ danh sách waypoint (dùng khi dựng path bằng code).</summary>
        public void SetWaypoints(List<Transform> wps)
        {
            waypoints = wps ?? new List<Transform>();
        }

        /// <summary>Chỉ số waypoint xuất phát của một phe.</summary>
        public int StartIndex(Team team) => team == Team.Player ? 0 : waypoints.Count - 1;

        /// <summary>Chỉ số waypoint đích (sát base địch) của một phe.</summary>
        public int EndIndex(Team team) => team == Team.Player ? waypoints.Count - 1 : 0;

        /// <summary>Bước nhảy index khi một phe tiến về phía địch (+1 hoặc -1).</summary>
        public int Step(Team team) => team == Team.Player ? 1 : -1;

        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Count == 0) return;
            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] == null) continue;
                Gizmos.DrawSphere(waypoints[i].position, 0.2f);
                if (i + 1 < waypoints.Count && waypoints[i + 1] != null)
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }
}
