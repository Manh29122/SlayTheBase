using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SlayTheTower.Data
{
    /// <summary>
    /// Lưu / nạp danh sách UNIT do người chơi tạo (JSON ở persistentDataPath/playerunits.json).
    /// </summary>
    public static class PlayerUnitStorage
    {
        [System.Serializable]
        private class Wrapper { public List<PlayerUnitData> units = new(); }

        private static string FilePath => Path.Combine(Application.persistentDataPath, "playerunits.json");

        public static List<PlayerUnitData> LoadAll()
        {
            if (!File.Exists(FilePath)) return new List<PlayerUnitData>();
            var w = JsonUtility.FromJson<Wrapper>(File.ReadAllText(FilePath));
            return w?.units ?? new List<PlayerUnitData>();
        }

        public static void SaveAll(List<PlayerUnitData> units)
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(new Wrapper { units = units }, true));
        }

        public static void Add(PlayerUnitData unit)
        {
            var all = LoadAll();
            all.Add(unit);
            SaveAll(all);
        }

        public static PlayerUnitData GetById(string id)
        {
            foreach (var u in LoadAll())
                if (u.id == id) return u;
            return null;
        }
    }
}
