using SlayTheTower.Data;

namespace SlayTheTower
{
    /// <summary>
    /// Dữ liệu level được chọn ở màn map, truyền sang Combat scene.
    /// Là static nên SỐNG QUA load scene (không bị huỷ khi đổi scene).
    /// </summary>
    public static class LevelSession
    {
        public static int SelectedId { get; private set; } = -1;
        public static WaveDefinition SelectedWave { get; private set; }

        public static void Set(int levelId, WaveDefinition wave)
        {
            SelectedId = levelId;
            SelectedWave = wave;
        }
    }
}
