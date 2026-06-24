using UnityEngine;

namespace SlayTheTower.Data
{
    /// <summary>
    /// Thư viện ảnh nền lá bài (ship sẵn trong app). Người chơi chọn 1 ảnh khi tạo thẻ;
    /// dữ liệu chỉ lưu <c>id</c>, lúc load tra ngược ra Sprite qua <see cref="Get"/>.
    /// Tạo asset: Create > Slay The Tower > Background Library.
    /// </summary>
    [CreateAssetMenu(fileName = "BackgroundLibrary", menuName = "Slay The Tower/Background Library", order = 22)]
    public class BackgroundLibrary : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            public string id;
            public Sprite sprite;
        }

        public Entry[] backgrounds;

        public int Count => backgrounds != null ? backgrounds.Length : 0;
        public Entry GetAt(int index) => backgrounds[index];

        public Sprite Get(string id)
        {
            if (backgrounds == null) return null;
            foreach (var e in backgrounds)
                if (e.id == id) return e.sprite;
            return null;
        }
    }
}
