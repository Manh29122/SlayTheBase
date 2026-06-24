using UnityEngine;

namespace SlayTheTower
{
    /// <summary>
    /// Một đám mây nền: trôi ngang theo vận tốc cho trước, tự huỷ khi ra khỏi mép despawn.
    /// Do <see cref="CloudSpawner"/> tạo và khởi tạo.
    /// </summary>
    public class Cloud : MonoBehaviour
    {
        private CloudSpawner _owner;
        private float _vx;
        private float _despawnX;
        private bool _movingLeft;
        private bool _initialized;

        public void Init(CloudSpawner owner, float velocityX, float despawnX)
        {
            _owner = owner;
            _vx = velocityX;
            _despawnX = despawnX;
            _movingLeft = velocityX < 0f;
            _initialized = true;
        }

        private void Update()
        {
            if (!_initialized) return;

            transform.position += new Vector3(_vx * Time.deltaTime, 0f, 0f);

            bool gone = _movingLeft ? transform.position.x <= _despawnX
                                    : transform.position.x >= _despawnX;
            if (gone)
            {
                if (_owner != null) _owner.OnCloudDespawned();
                Destroy(gameObject);
            }
        }
    }
}
