using UnityEngine;
using SlayTheTower.Data;

namespace SlayTheTower
{
    /// <summary>
    /// Công trình (base) của một phe: có máu, là nơi spawn quân, và bị đánh được.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class Base : MonoBehaviour
    {
        [SerializeField] private Team team;
        [SerializeField] private BaseDefinition definition;
        [Tooltip("Nơi quân của phe này xuất hiện. Để trống = dùng vị trí của base.")]
        [SerializeField] private Transform spawnPoint;

        public Team Team => team;
        public Health Health { get; private set; }
        public Vector3 SpawnPosition => spawnPoint != null ? spawnPoint.position : transform.position;

        private void Awake()
        {
            Health = GetComponent<Health>();
            Health.Setup(definition != null ? definition.maxHP : 1000f);
        }

        /// <summary>Thiết lập base bằng code (dùng cho spawn/bootstrap).</summary>
        public void Configure(Team t, BaseDefinition def, Transform spawn)
        {
            team = t;
            definition = def;
            spawnPoint = spawn;
            if (Health == null) Health = GetComponent<Health>();
            Health.Setup(def != null ? def.maxHP : 1000f);
        }
    }
}
