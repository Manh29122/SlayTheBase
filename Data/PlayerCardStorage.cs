using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SlayTheTower.Data
{
    /// <summary>
    /// Lưu / nạp danh sách thẻ người chơi tạo (JSON ở persistentDataPath/playercards.json).
    /// Chạy được trên build mobile — KHÔNG dùng AssetDatabase/prefab.
    /// </summary>
    public static class PlayerCardStorage
    {
        [System.Serializable]
        private class Wrapper { public List<PlayerCardData> cards = new(); }

        private static string FilePath => Path.Combine(Application.persistentDataPath, "playercards.json");

        public static List<PlayerCardData> LoadAll()
        {
            if (!File.Exists(FilePath)) return new List<PlayerCardData>();
            var w = JsonUtility.FromJson<Wrapper>(File.ReadAllText(FilePath));
            return w?.cards ?? new List<PlayerCardData>();
        }

        public static void SaveAll(List<PlayerCardData> cards)
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(new Wrapper { cards = cards }, true));
        }

        public static void Add(PlayerCardData card)
        {
            var all = LoadAll();
            all.Add(card);
            SaveAll(all);
        }
    }
}
