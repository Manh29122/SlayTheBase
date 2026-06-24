using UnityEngine;

namespace SlayTheTower.Drawing
{
    /// <summary>
    /// Logic vẽ pixel lên một Texture2D: tô điểm/đoạn (Bresenham), cọ vuông kích thước N,
    /// xoá, và xuất PNG. Texture dùng FilterMode.Point để giữ nét pixel khi phóng to.
    /// </summary>
    public class PixelPainter
    {
        public Texture2D Texture { get; }
        public int Width { get; }
        public int Height { get; }

        private readonly Color32[] _pixels;

        public PixelPainter(int width, int height, Color32 fill)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);

            Texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            _pixels = new Color32[Width * Height];
            Fill(fill);
        }

        public void Fill(Color32 c)
        {
            for (int i = 0; i < _pixels.Length; i++) _pixels[i] = c;
            Apply();
        }

        public void SetPixel(int x, int y, Color32 c)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return;
            _pixels[y * Width + x] = c;
        }

        /// <summary>Tô một đoạn từ (x0,y0) đến (x1,y1) bằng cọ vuông — gọi mỗi lần kéo.</summary>
        public void PaintLine(int x0, int y0, int x1, int y1, Color32 c, int brush)
        {
            int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                Stamp(x0, y0, c, brush);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
            Apply();
        }

        private void Stamp(int cx, int cy, Color32 c, int brush)
        {
            int r = Mathf.Max(1, brush);
            int half = (r - 1) / 2;
            for (int oy = 0; oy < r; oy++)
                for (int ox = 0; ox < r; ox++)
                    SetPixel(cx + ox - half, cy + oy - half, c);
        }

        /// <summary>Nạp pixel từ ảnh khác (vd ảnh đã lưu) vào khung. Copy vùng giao nhau,
        /// phần dư để trong suốt. Dùng khi load ảnh cũ về vẽ tiếp.</summary>
        public void LoadPixels(Color32[] src, int srcWidth, int srcHeight)
        {
            for (int i = 0; i < _pixels.Length; i++) _pixels[i] = new Color32(0, 0, 0, 0);
            int w = Mathf.Min(Width, srcWidth);
            int h = Mathf.Min(Height, srcHeight);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    _pixels[y * Width + x] = src[y * srcWidth + x];
            Apply();
        }

        public void Apply()
        {
            Texture.SetPixels32(_pixels);
            Texture.Apply();
        }

        public byte[] EncodePng() => Texture.EncodeToPNG();
    }
}
