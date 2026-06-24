using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SlayTheTower.Data
{
    /// <summary>
    /// Lưu / nạp DECK của người chơi = danh sách card id (JSON ở persistentDataPath/playerdeck.json).
    /// Cùng 1 id có thể xuất hiện nhiều lần (vd 3 lá Chiến binh trong deck).
    /// </summary>
    public static class PlayerDeckStorage
    {
        [System.Serializable] private class Wrapper { public List<string> cardIds = new(); }

        private static string FilePath => Path.Combine(Application.persistentDataPath, "playerdeck.json");

        public static List<string> Load()
        {
            if (!File.Exists(FilePath)) return new List<string>();
            var w = JsonUtility.FromJson<Wrapper>(File.ReadAllText(FilePath));
            return w?.cardIds ?? new List<string>();
        }

        public static void Save(List<string> cardIds)
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(new Wrapper { cardIds = cardIds }, true));
        }
    }
}
