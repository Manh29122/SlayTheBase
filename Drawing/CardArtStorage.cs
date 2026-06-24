using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SlayTheTower.Drawing
{
    /// <summary>
    /// Lưu / nạp artwork thẻ do người chơi vẽ (PNG trong persistentDataPath/cards).
    /// </summary>
    public static class CardArtStorage
    {
        private static string Dir => Path.Combine(Application.persistentDataPath, "cards");

        public static string Save(string fileName, byte[] png)
        {
            Directory.CreateDirectory(Dir);
            string path = Path.Combine(Dir, fileName + ".png");
            File.WriteAllBytes(path, png);
            Debug.Log(path);
            return path;
        }

        public static Texture2D LoadTexture(string fileName)
        {
            string path = Path.Combine(Dir, fileName + ".png");
            if (!File.Exists(path)) return null;

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.LoadImage(File.ReadAllBytes(path)); // tự resize theo PNG
            return tex;
        }

        public static Sprite Load(string fileName)
        {
            var tex = LoadTexture(fileName);
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), Mathf.Max(tex.width, tex.height));
        }

        public static List<string> ListSaved()
        {
            if (!Directory.Exists(Dir)) return new List<string>();
            return Directory.GetFiles(Dir, "*.png")
                .Select(Path.GetFileNameWithoutExtension)
                .ToList();
        }

        public static string Folder => Dir;
    }
}
