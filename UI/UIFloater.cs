using System;
using UnityEngine;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Cho một phần tử UI (RectTransform) trôi ngang theo vận tốc, tự huỷ khi ra khỏi mép despawn.
    /// Do <see cref="CardCloudSpawner"/> tạo & khởi tạo.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIFloater : MonoBehaviour
    {
        private RectTransform _rt;
        private float _vx;
        private float _despawnX;
        private bool _movingLeft;
        private bool _initialized;
        private Action _onDespawn;

        public void Init(float velocityX, float despawnX, Action onDespawn)
        {
            _rt = (RectTransform)transform;
            _vx = velocityX;
            _despawnX = despawnX;
            _movingLeft = velocityX < 0f;
            _onDespawn = onDespawn;
            _initialized = true;
        }

        private void Update()
        {
            if (!_initialized) return;

            _rt.anchoredPosition += new Vector2(_vx * Time.deltaTime, 0f);

            bool gone = _movingLeft ? _rt.anchoredPosition.x <= _despawnX
                                    : _rt.anchoredPosition.x >= _despawnX;
            if (gone)
            {
                _onDespawn?.Invoke();
                Destroy(gameObject);
            }
        }
    }
}
