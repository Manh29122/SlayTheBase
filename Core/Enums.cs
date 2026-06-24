namespace SlayTheTower
{
    /// <summary>Phe của một đơn vị / công trình.</summary>
    public enum Team
    {
        Player,
        Enemy
    }

    /// <summary>Vai trò / kiểu hành vi của đơn vị.</summary>
    public enum UnitRole
    {
        Melee,   // Cận chiến: đấu sĩ, sát thủ, chiến binh
        Ranged,  // Tầm xa: cung thủ, wizard
        Healer,  // Hồi máu cho đồng minh
        Support  // Hỗ trợ: phát aura buff (tăng energy, công, thủ...)
    }

    /// <summary>Cách đơn vị gây sát thương.</summary>
    public enum AttackType
    {
        Melee,   // Đánh giáp lá cà
        Ranged   // Bắn đạn (projectile)
    }

    /// <summary>Loại thẻ bài.</summary>
    public enum CardType
    {
        SummonUnit,  // Triệu hồi đơn vị quân
        Buff         // Kích hoạt hiệu ứng buff
    }

    /// <summary>Độ hiếm của thẻ — dùng cho shop & bảng rớt.</summary>
    public enum Rarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>Phạm vi tác động của một buff.</summary>
    public enum BuffScope
    {
        AllAllyUnits,   // Toàn bộ quân đồng minh đang có trên sân
        AllyBase,       // Base của người chơi
        AreaAroundCast, // Vùng tròn quanh điểm thả thẻ
        SingleAllyUnit  // Một đơn vị đồng minh
    }

    /// <summary>Loại hiệu ứng mà một thẻ có thể kích hoạt (xem CardEffectDefinition).</summary>
    public enum CardEffectType
    {
        ManaRegenAdd,          // +x mana hồi mỗi giây
        InstantHealUnits,      // Hồi máu tức thì cho toàn quân (theo % máu tối đa và/hoặc flat)
        UnitStatBuff,          // Áp BuffDefinition cho toàn quân (công/thủ/tốc/thần thánh...)
        ZoneAllyImmune,        // Tạo vùng quân ta không bị sát thương
        ZoneDamageEnemy,       // Tạo vùng gây sát thương tức thì cho địch
        ZoneSlowEnemy,         // Tạo vùng làm chậm địch
        DrawCards,             // Rút thêm N lá
        ReturnRandomAndDraw,   // Trả N lá ngẫu nhiên từ tay về deck rồi rút M lá
        RecycleHandDrawTop,    // Úp cả tay xuống đáy deck, rút N lá trên cùng
        OnDrawAttackBuff,      // Mỗi lần rút bài: +x công cho toàn quân (cộng dồn)
        BaseSacrificeHeal,     // Trừ x% máu base, hồi y% máu toàn quân
        EnergyCostMultiplier,  // Nhân chi phí energy của bài (vd 1.5 = +50%)
        HandLimitDelta,        // Tăng/giảm giới hạn số lá trên tay (kèm chặn min/max)
        EnergyBurst,           // +X energy NGAY lập tức
        MaxEnergyAdd,          // Nâng trần energy tối đa thêm X
        NextCardFree,          // N lá đánh kế tiếp tốn 0 energy
        UpgradeRandomCard,     // Nâng cấp N lá ngẫu nhiên trên tay cả run (giảm cost + tăng summon)
        ZoneStun,              // Choáng quân địch trong vùng X giây
        ZoneDamageOverTime,    // Vùng gây sát thương mỗi giây cho địch (DoT)
        Meteor,                // Thiên thạch: sau delay giáng sát thương AoE xuống điểm thả
        ExecuteEnemies         // Giết ngay địch dưới X% máu trong vùng
    }
}
