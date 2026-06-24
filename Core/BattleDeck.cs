using System.Collections.Generic;
using UnityEngine;
using SlayTheTower.Data;

namespace SlayTheTower
{
    /// <summary>
    /// Bộ bài runtime: chỉ quản lý DRAW PILE (chồng rút) + giới hạn tay + hệ số chi phí.
    /// Lá đánh xong được trả về cuối chồng (cycling kiểu Clash Royale). Tay bài do HandView giữ.
    /// </summary>
    public class BattleDeck : MonoBehaviour
    {
        public int HandLimit { get; private set; } = 4;
        public float CostMultiplier { get; private set; } = 1f;
        public int DrawPileCount => _drawPile.Count;
        public int FreeCharges => _freeCharges;
        public bool HasFreeCard => _freeCharges > 0;

        private readonly List<CardDefinition> _drawPile = new();
        private readonly Dictionary<CardDefinition, int> _upgrades = new();
        private int _freeCharges;

        public void Initialize(IEnumerable<CardDefinition> cards, int handLimit)
        {
            _drawPile.Clear();
            if (cards != null) _drawPile.AddRange(cards);
            HandLimit = Mathf.Max(1, handLimit);
            CostMultiplier = 1f;
            _freeCharges = 0;
            _upgrades.Clear();
            Shuffle();
        }

        public void Shuffle()
        {
            for (int i = _drawPile.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (_drawPile[i], _drawPile[j]) = (_drawPile[j], _drawPile[i]);
            }
        }

        /// <summary>Rút lá trên cùng (null nếu hết bài).</summary>
        public CardDefinition DrawOne()
        {
            if (_drawPile.Count == 0) return null;
            var c = _drawPile[0];
            _drawPile.RemoveAt(0);
            return c;
        }

        public void ReturnToBottom(CardDefinition card)
        {
            if (card != null) _drawPile.Add(card);
        }

        /// <summary>Chi phí energy thực tế của lá (đã trừ nâng cấp + nhân hệ số).</summary>
        public float EffectiveCost(CardDefinition card)
        {
            if (card == null) return 0f;
            float baseCost = Mathf.Max(0, card.energyCost - GetUpgrade(card));
            return baseCost * Mathf.Max(0f, CostMultiplier);
        }

        public void MultiplyCost(float multiplier)
            => CostMultiplier = Mathf.Max(0f, CostMultiplier * multiplier);

        public void ChangeHandLimit(int delta, int min, int max)
            => HandLimit = Mathf.Clamp(HandLimit + delta, min, max);

        // ----- Lá miễn phí -----
        public void GrantFreeCards(int n) { if (n > 0) _freeCharges += n; }
        public void ConsumeFreeCard() { if (_freeCharges > 0) _freeCharges--; }

        // ----- Nâng cấp lá (cả run) -----
        public void Upgrade(CardDefinition card, int levels = 1)
        {
            if (card == null || levels <= 0) return;
            _upgrades.TryGetValue(card, out int cur);
            _upgrades[card] = cur + levels;
        }

        public int GetUpgrade(CardDefinition card)
            => card != null && _upgrades.TryGetValue(card, out int lv) ? lv : 0;
    }
}
