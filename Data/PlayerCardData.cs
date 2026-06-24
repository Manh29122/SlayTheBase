namespace SlayTheTower.Data
{
    /// <summary>
    /// Dữ liệu một lá bài do NGƯỜI CHƠI tạo lúc chạy (mobile) — plain class để JsonUtility lưu được.
    /// KHÔNG phải ScriptableObject asset: lưu JSON trong persistentDataPath, art lưu PNG riêng.
    /// </summary>
    [System.Serializable]
    public class PlayerCardData
    {
        public string id;
        public string displayName;
        public string description;
        public int energyCost = 3;
        [UnityEngine.Tooltip("Giá mua/bán trong shop (vàng).")]
        public int shopPrice = 50;
        public Rarity rarity = Rarity.Common;
        public CardType cardType = CardType.SummonUnit;

        [UnityEngine.Tooltip("Id ảnh nền chọn từ BackgroundLibrary.")]
        public string backgroundId;
        [UnityEngine.Tooltip("Màu chữ tên / mô tả (JsonUtility lưu được Color).")]
        public UnityEngine.Color nameColor = UnityEngine.Color.white;
        public UnityEngine.Color descColor = UnityEngine.Color.white;

        [UnityEngine.Tooltip("Tên file PNG (trong CardArtStorage) chứa ảnh người chơi vẽ.")]
        public string artFile;

        [UnityEngine.Tooltip("Khi cardType = SummonUnit: id của UnitDefinition sẽ triệu hồi.")]
        public string unitId;
        public int summonCount = 1;

        [UnityEngine.Tooltip("Khi cardType = Buff: danh sách id hiệu ứng (tra trong registry).")]
        public string[] effectIds;
    }
}
