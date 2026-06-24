using UnityEngine;

namespace SlayTheTower
{
    /// <summary>
    /// Sinh các đám mây trôi từ NGOÀI màn hình vào rồi qua bên kia. Vùng spawn theo trục Y
    /// chỉnh bằng <see cref="yMin"/>/<see cref="yMax"/> (toạ độ world) — vẽ gizmo cyan để dễ canh.
    /// Nếu không gán sprite/prefab thì tự tạo sprite mây mềm để chạy thử ngay.
    /// Gắn lên 1 GameObject rỗng trong scene là chạy.
    /// </summary>
    public class CloudSpawner : MonoBehaviour
    {
        [Header("Hình mây (ưu tiên Prefabs; trống cả hai -> tự tạo)")]
        [SerializeField] private GameObject[] cloudPrefabs;
        [SerializeField] private Sprite[] cloudSprites;

        [Header("Vùng spawn theo trục Y (world)")]
        [SerializeField] private float yMin = 1f;
        [SerializeField] private float yMax = 4f;

        [Header("Nhịp spawn")]
        [SerializeField] private float intervalMin = 1.5f;
        [SerializeField] private float intervalMax = 4f;
        [Tooltip("Số mây tối đa cùng lúc.")]
        [SerializeField] private int maxClouds = 12;
        [Tooltip("Rải sẵn vài đám mây ngay khi bắt đầu.")]
        [SerializeField] private bool prewarm = true;

        [Header("Chuyển động")]
        [Tooltip("Bay từ phải sang trái (tắt = trái sang phải).")]
        [SerializeField] private bool moveLeft = true;
        [SerializeField] private float speedMin = 0.4f;
        [SerializeField] private float speedMax = 1.2f;
        [SerializeField] private Vector2 scaleRange = new Vector2(0.8f, 1.6f);
        [Tooltip("Khoảng spawn/despawn lố ra ngoài mép màn hình.")]
        [SerializeField] private float offscreenMargin = 2f;

        [Header("Hiển thị")]
        [SerializeField] private string sortingLayer = "Default";
        [SerializeField] private int sortingOrder = -20;
        [SerializeField] private Color tint = new Color(1f, 1f, 1f, 0.9f);
        [Tooltip("Camera để tính mép màn hình (để trống = Camera.main).")]
        [SerializeField] private Camera cam;

        private float _timer;
        private int _count;
        private static Sprite _fallback;

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
        }

        private void Start()
        {
            if (prewarm)
            {
                int n = Mathf.Min(maxClouds, 5);
                for (int i = 0; i < n; i++) SpawnCloud(true);
            }
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _timer = Random.Range(intervalMin, intervalMax);
                if (_count < maxClouds) SpawnCloud(false);
            }
        }

        public void OnCloudDespawned() => _count = Mathf.Max(0, _count - 1);

        private void SpawnCloud(bool atRandomX)
        {
            GetEdges(out float left, out float right);
            float y = Random.Range(Mathf.Min(yMin, yMax), Mathf.Max(yMin, yMax));

            float spawnX, despawnX;
            if (moveLeft)
            {
                spawnX = atRandomX ? Random.Range(left, right) : right + offscreenMargin;
                despawnX = left - offscreenMargin;
            }
            else
            {
                spawnX = atRandomX ? Random.Range(left, right) : left - offscreenMargin;
                despawnX = right + offscreenMargin;
            }

            var go = CreateCloudObject();
            go.transform.position = new Vector3(spawnX, y, 0f);
            float s = Random.Range(scaleRange.x, scaleRange.y);
            go.transform.localScale = new Vector3(s, s, 1f);

            float speed = Random.Range(speedMin, speedMax);
            var cloud = go.GetComponent<Cloud>();
            if (cloud == null) cloud = go.AddComponent<Cloud>();
            cloud.Init(this, moveLeft ? -speed : speed, despawnX);
            _count++;
        }

        private GameObject CreateCloudObject()
        {
            if (cloudPrefabs != null && cloudPrefabs.Length > 0)
            {
                var prefab = cloudPrefabs[Random.Range(0, cloudPrefabs.Length)];
                var go = Instantiate(prefab);
                go.SetActive(true);
                return go;
            }

            var cloud = new GameObject("Cloud");
            var sr = cloud.AddComponent<SpriteRenderer>();
            sr.sprite = (cloudSprites != null && cloudSprites.Length > 0)
                ? cloudSprites[Random.Range(0, cloudSprites.Length)]
                : FallbackSprite();
            sr.color = tint;
            if (!string.IsNullOrEmpty(sortingLayer)) sr.sortingLayerName = sortingLayer;
            sr.sortingOrder = sortingOrder;
            return cloud;
        }

        private void GetEdges(out float left, out float right)
        {
            var c = cam != null ? cam : Camera.main;
            if (c != null && c.orthographic)
            {
                float halfH = c.orthographicSize;
                float halfW = halfH * c.aspect;
                float cx = c.transform.position.x;
                left = cx - halfW;
                right = cx + halfW;
            }
            else { left = -9f; right = 9f; }
        }

        // Sprite mây mềm tạo runtime (dùng khi chưa gán hình thật).
        private static Sprite FallbackSprite()
        {
            if (_fallback != null) return _fallback;

            const int w = 128, h = 72;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[w * h];

            Vector3[] puffs =
            {
                new(38, 32, 22), new(64, 42, 28), new(90, 32, 22), new(52, 28, 17), new(78, 28, 17)
            };

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float a = 0f;
                    foreach (var p in puffs)
                    {
                        float d = Mathf.Sqrt((x - p.x) * (x - p.x) + (y - p.y) * (y - p.y));
                        float t = 1f - d / p.z;
                        if (t > a) a = t;
                    }
                    a = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(a * 1.3f));
                    px[y * w + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            _fallback = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 64f);
            return _fallback;
        }

        private void OnDrawGizmosSelected()
        {
            var c = cam != null ? cam : Camera.main;
            float left = -10f, right = 10f;
            if (c != null && c.orthographic)
            {
                float halfW = c.orthographicSize * c.aspect;
                left = c.transform.position.x - halfW - offscreenMargin;
                right = c.transform.position.x + halfW + offscreenMargin;
            }
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(new Vector3(left, yMin), new Vector3(right, yMin));
            Gizmos.DrawLine(new Vector3(left, yMax), new Vector3(right, yMax));
            Gizmos.color = new Color(0f, 1f, 1f, 0.08f);
            Gizmos.DrawCube(new Vector3((left + right) * 0.5f, (yMin + yMax) * 0.5f, 0f),
                new Vector3(right - left, Mathf.Abs(yMax - yMin), 0.01f));
        }
    }
}
