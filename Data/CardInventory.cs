using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SlayTheTower.Data
{
    /// <summary>
    /// Kho thẻ NGƯỜI CHƠI SỞ HỮU: đếm số lá mỗi loại (theo card id). Lưu JSON ở
    /// persistentDataPath/cardinventory.json. Vd: "warrior" -> 3 (đang có 3 lá Chiến binh).
    /// </summary>
    public static class CardInventory
    {
        [Serializable] private class Entry { public string id; public int count; }
        [Serializable] private class Wrapper { public List<Entry> entries = new(); }

        private static string FilePath => Path.Combine(Application.persistentDataPath, "cardinventory.json");
        private static Dictionary<string, int> _counts;

        /// <summary>Bắn ra mỗi khi số lượng đổi (để UI/card cập nhật).</summary>
        public static event Action OnChanged;

        private static Dictionary<string, int> Counts
        {
            get { if (_counts == null) Load(); return _counts; }
        }

        public static int GetCount(string id)
            => !string.IsNullOrEmpty(id) && Counts.TryGetValue(id, out var c) ? c : 0;

        public static bool Has(string id) => GetCount(id) > 0;

        /// <summary>Bản sao toàn bộ kho (id -> số lá) để duyệt (vd dựng màn tạo deck).</summary>
        public static Dictionary<string, int> GetAll() => new Dictionary<string, int>(Counts);

        /// <summary>Cộng (n>0) / trừ (n<0) số lá. Tự xoá entry khi về 0.</summary>
        public static void Add(string id, int n = 1)
        {
            if (string.IsNullOrEmpty(id) || n == 0) return;
            Counts.TryGetValue(id, out var c);
            c = Mathf.Max(0, c + n);
            if (c == 0) Counts.Remove(id);
            else Counts[id] = c;
            Save();
        }

        /// <summary>Bớt n lá nếu đủ. Trả về false nếu không đủ.</summary>
        public static bool TryRemove(string id, int n = 1)
        {
            if (GetCount(id) < n) return false;
            Add(id, -n);
            return true;
        }

        private static void Load()
        {
            _counts = new Dictionary<string, int>();
            if (!File.Exists(FilePath)) return;
            var w = JsonUtility.FromJson<Wrapper>(File.ReadAllText(FilePath));
            if (w?.entries == null) return;
            foreach (var e in w.entries)
                if (!string.IsNullOrEmpty(e.id)) _counts[e.id] = e.count;
        }

        private static void Save()
        {
            var w = new Wrapper();
            foreach (var kv in Counts) w.entries.Add(new Entry { id = kv.Key, count = kv.Value });
            File.WriteAllText(FilePath, JsonUtility.ToJson(w, true));
            OnChanged?.Invoke();
        }
    }
}
