using System.Collections.Generic;
using UnityEngine;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Spawn prefab lá bài (UI/RectTransform) trôi ngang như mây trong nền menu.
    /// Vùng di chuyển theo trục Y chỉnh được; spawn từ NGOÀI màn hình vào rồi trôi qua bên kia.
    /// Gắn lên một RectTransform con của Canvas (nên để full màn hình, nằm DƯỚI các UI khác).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class CardCloudSpawner : MonoBehaviour
    {
        [Header("Prefab lá bài (RectTransform)")]
        [SerializeField] private List<GameObject> cardPrefabs = new();

        [Header("Vùng spawn theo trục Y (anchored, so với vùng này)")]
        [SerializeField] private float yMin = -150f;
        [SerializeField] private float yMax = 200f;

        [Header("Nhịp")]
        [SerializeField] private float intervalMin = 1.5f;
        [SerializeField] private float intervalMax = 4f;
        [SerializeField] private int maxCount = 10;
        [SerializeField] private bool prewarm = true;

        [Header("Chuyển động")]
        [Tooltip("Trôi từ phải sang trái (tắt = trái sang phải).")]
        [SerializeField] private bool moveLeft = true;
        [Tooltip("Thời gian (giây) để 1 lá trôi hết bề ngang màn hình. >0 sẽ ƯU TIÊN (bỏ qua Speed).")]
        [SerializeField] private float lifetime = 0f;
        [Tooltip("Tốc độ (đơn vị anchored/giây). Chỉ dùng khi Lifetime = 0.")]
        [SerializeField] private float speedMin = 60f;
        [SerializeField] private float speedMax = 160f;
        [SerializeField] private Vector2 scaleRange = new Vector2(0.8f, 1.2f);
        [Tooltip("Khoảng spawn/despawn lố ra ngoài mép.")]
        [SerializeField] private float offscreenMargin = 250f;

        private RectTransform _area;
        private float _timer;
        private int _count;

        private void Awake() => _area = (RectTransform)transform;

        private void Start()
        {
            if (prewarm)
            {
                int n = Mathf.Min(maxCount, 4);
                for (int i = 0; i < n; i++) Spawn(true);
            }
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _timer = Random.Range(intervalMin, intervalMax);
                if (_count < maxCount) Spawn(false);
            }
        }

        private void Spawn(bool atRandomX)
        {
            if (cardPrefabs == null || cardPrefabs.Count == 0) return;
            var prefab = cardPrefabs[Random.Range(0, cardPrefabs.Count)];
            if (prefab == null) return;

            float halfW = _area.rect.width * 0.5f;
            float spawnX, despawnX;
            if (moveLeft)
            {
                spawnX = atRandomX ? Random.Range(-halfW, halfW) : halfW + offscreenMargin;
                despawnX = -halfW - offscreenMargin;
            }
            else
            {
                spawnX = atRandomX ? Random.Range(-halfW, halfW) : -halfW - offscreenMargin;
                despawnX = halfW + offscreenMargin;
            }
            float y = Random.Range(Mathf.Min(yMin, yMax), Mathf.Max(yMin, yMax));

            var go = Instantiate(prefab, _area);
            go.SetActive(true);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = new Vector2(spawnX, y);
            float s = Random.Range(scaleRange.x, scaleRange.y);
            rt.localScale = new Vector3(s, s, 1f);

            // Lá nền không chặn click vào menu.
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;

            // Lifetime > 0: tính tốc độ để lá trôi hết bề ngang màn hình trong đúng số giây đó.
            float fullDistance = _area.rect.width + 2f * offscreenMargin;
            float speed = lifetime > 0f ? fullDistance / lifetime : Random.Range(speedMin, speedMax);

            var floater = go.GetComponent<UIFloater>();
            if (floater == null) floater = go.AddComponent<UIFloater>();
            floater.Init(moveLeft ? -speed : speed, despawnX, OnDespawned);
            _count++;
        }

        private void OnDespawned() => _count = Mathf.Max(0, _count - 1);
    }
}
