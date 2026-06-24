using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SlayTheTower.Drawing;

namespace SlayTheTower.UI
{
    /// <summary>
    /// Quản 1 chuỗi frame hoạt ảnh (vd Walk hoặc Attack): giữ danh sách id ảnh THEO THỨ TỰ
    /// và hiện thumbnail trong Container (nên có Horizontal/GridLayoutGroup).
    /// Bấm "thêm frame" → mở picker → chọn ảnh → AddFrame nối vào cuối.
    /// </summary>
    public class FrameSequenceBuilder : MonoBehaviour
    {
        [Tooltip("Nơi hiện thumbnail các frame (có Layout Group).")]
        [SerializeField] private Transform container;

        private readonly List<string> _ids = new();

        public IReadOnlyList<string> FrameIds => _ids;
        public int Count => _ids.Count;
        public string[] ToArray() => _ids.ToArray();

        public void AddFrame(string id)
        {
            if (string.IsNullOrEmpty(id) || container == null) return;
            _ids.Add(id);

            var go = new GameObject(id, typeof(Image));
            go.transform.SetParent(container, false);
            var img = go.GetComponent<Image>();
            img.sprite = CardArtStorage.Load(id);
            img.preserveAspect = true;
        }

        public void RemoveLast()
        {
            if (_ids.Count == 0) return;
            _ids.RemoveAt(_ids.Count - 1);
            if (container != null && container.childCount > 0)
                Destroy(container.GetChild(container.childCount - 1).gameObject);
        }

        public void Clear()
        {
            _ids.Clear();
            if (container == null) return;
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }

        public void SetFrames(IEnumerable<string> ids)
        {
            Clear();
            if (ids == null) return;
            foreach (var id in ids) AddFrame(id);
        }
    }
}
