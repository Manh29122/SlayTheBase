using System.Collections.Generic;
using UnityEngine;

namespace SlayTheTower
{
    /// <summary>
    /// Tiến độ hoàn thành level — lưu theo TẬP id đã qua (hỗ trợ map rẽ nhánh, id không cần tuần tự).
    /// Việc mở khoá do prerequisite của từng node quyết định (xem LevelNode), không phải ở đây.
    /// Lưu bằng PlayerPrefs (1 chuỗi CSV id).
    /// </summary>
    public static class LevelProgress
    {
        private const string Key = "completed_level_ids";
        private static HashSet<int> _cache;

        private static HashSet<int> Done()
        {
            if (_cache != null) return _cache;
            _cache = new HashSet<int>();
            string s = PlayerPrefs.GetString(Key, "");
            foreach (var part in s.Split(','))
                if (int.TryParse(part, out int id)) _cache.Add(id);
            return _cache;
        }

        private static void Save()
        {
            PlayerPrefs.SetString(Key, string.Join(",", Done()));
            PlayerPrefs.Save();
        }

        /// <summary>Level (theo id) đã hoàn thành chưa.</summary>
        public static bool IsCompleted(int levelId) => Done().Contains(levelId);

        /// <summary>Đánh dấu hoàn thành 1 level.</summary>
        public static void Complete(int levelId)
        {
            if (Done().Add(levelId)) Save();
        }

        /// <summary>Xoá toàn bộ tiến độ (cho test).</summary>
        public static void ResetProgress()
        {
            Done().Clear();
            Save();
        }
    }
}
