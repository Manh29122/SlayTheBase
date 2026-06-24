using UnityEngine;
using UnityEngine.SceneManagement;
using SlayTheTower.Data;

namespace SlayTheTower
{
    /// <summary>
    /// Đặt trong COMBAT scene. Đọc level được chọn từ <see cref="LevelSession"/> để dựng trận.
    /// Khi THẮNG, gọi <see cref="ReportWin"/> để mở khoá level kế. <see cref="ReturnToMap"/> để về map.
    /// </summary>
    public class CombatLevelLoader : MonoBehaviour
    {
        [Tooltip("Wave dùng khi vào thẳng Combat scene để test (không qua map).")]
        [SerializeField] private WaveDefinition fallbackWave;
        [Tooltip("Tên scene map để quay lại sau trận.")]
        [SerializeField] private string mapSceneName = "LevelMap";

        public WaveDefinition Wave { get; private set; }
        public int LevelId { get; private set; }

        private void Awake()
        {
            LevelId = LevelSession.SelectedId;
            Wave = LevelSession.SelectedWave != null ? LevelSession.SelectedWave : fallbackWave;

            if (Wave == null)
                Debug.LogWarning("[CombatLevelLoader] Không có level nào được chọn và chưa gán Fallback Wave.", this);

            // TODO: dùng Wave để dựng trận — spawn địch theo Wave.spawns, set máu base, thời lượng...
        }

        /// <summary>Gọi khi người chơi THẮNG level hiện tại → đánh dấu hoàn thành (mở các node phụ thuộc).</summary>
        public void ReportWin()
        {
            if (LevelId >= 0) LevelProgress.Complete(LevelId);
        }

        /// <summary>Quay lại màn map.</summary>
        public void ReturnToMap()
        {
            if (!string.IsNullOrEmpty(mapSceneName)) SceneManager.LoadScene(mapSceneName);
        }
    }
}
