using UnityEngine;

namespace SlayTheTower
{
    /// <summary>
    /// Chữ nổi (vd số sát thương) bay lên rồi mờ dần và tự huỷ. Dùng TextMesh world-space,
    /// không cần prefab — gọi <see cref="Spawn"/> là xong.
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.8f;
        [SerializeField] private float riseSpeed = 1.6f;

        private float _t;
        private TextMesh _text;
        private Color _baseColor;

        public void Setup(string content, Color color)
        {
            if (_text == null) _text = GetComponent<TextMesh>();
            if (_text != null)
            {
                _text.text = content;
                _text.color = color;
            }
            _baseColor = color;
        }

        private void Update()
        {
            _t += Time.deltaTime;
            transform.position += Vector3.up * (riseSpeed * Time.deltaTime);

            if (_text != null)
            {
                var c = _baseColor;
                c.a = Mathf.Clamp01(1f - _t / lifetime);
                _text.color = c;
            }

            if (_t >= lifetime) Destroy(gameObject);
        }

        /// <summary>Tạo nhanh một floating text tại vị trí world.</summary>
        public static FloatingText Spawn(Vector3 position, string content, Color color)
        {
            var go = new GameObject("FloatingText");
            go.transform.position = position;

            var tm = go.AddComponent<TextMesh>();
            tm.fontSize = 64;
            tm.characterSize = 0.07f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = tm.font.material;
            mr.sortingOrder = 100; // hiện trên quân

            var ft = go.AddComponent<FloatingText>();
            ft._text = tm;
            ft.Setup(content, color);
            return ft;
        }
    }
}
