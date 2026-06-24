using UnityEngine;
using UnityEngine.SceneManagement;

namespace SlayTheTower
{
    /// <summary>
    /// Load scene theo tên. Gắn lên 1 GameObject rồi gọi từ Button.onClick / ButtonPressEffect.onPressComplete...
    /// - Wire trong Inspector: chọn <see cref="LoadScene"/> và GÕ tên scene vào ô string.
    /// - Hoặc điền sẵn <see cref="sceneName"/> rồi gọi <see cref="Load"/> (không cần tham số).
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        [Tooltip("Tên scene mặc định cho hàm Load() (không tham số).")]
        [SerializeField] private string sceneName;

        /// <summary>Load scene theo TÊN truyền vào (PHẢI có trong Build Settings).</summary>
        public void LoadScene(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning("[SceneLoader] Tên scene rỗng.", this);
                return;
            }
            if (!Application.CanStreamedLevelBeLoaded(name))
            {
                Debug.LogError($"[SceneLoader] Không tìm thấy scene '{name}'. " +
                               "Thêm scene vào File → Build Settings → Scenes In Build.", this);
                return;
            }
            SceneManager.LoadScene(name);
        }

        /// <summary>Load scene đã điền sẵn ở field <see cref="sceneName"/>.</summary>
        public void Load() => LoadScene(sceneName);

        /// <summary>Load lại scene hiện tại.</summary>
        public void ReloadCurrent() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
