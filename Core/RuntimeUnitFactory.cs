using System.Collections.Generic;
using UnityEngine;
using SlayTheTower.Data;
using SlayTheTower.Drawing;

namespace SlayTheTower
{
    /// <summary>
    /// Dựng <see cref="UnitDefinition"/> TRONG RAM từ <see cref="PlayerUnitData"/> (unit người chơi tạo).
    /// Nạp sprite frame từ PNG đã lưu; dựng cả viên đạn runtime cho unit tầm xa.
    /// Dùng được trên build mobile (CreateInstance, không tạo asset/prefab).
    /// </summary>
    public static class RuntimeUnitFactory
    {
        public static UnitDefinition Build(PlayerUnitData d)
        {
            var u = ScriptableObject.CreateInstance<UnitDefinition>();
            u.id = d.id;
            u.displayName = d.displayName;
            u.maxHP = d.maxHP;
            u.moveSpeed = d.moveSpeed;
            u.attackType = d.attackType;
            u.attackDamage = d.attackDamage;
            u.attackRange = d.attackRange;
            u.attackCooldown = d.attackCooldown;

            u.walkFrames = LoadSprites(d.walkFrameIds);
            u.walkDuration = d.walkDuration;
            u.attackFrames = LoadSprites(d.attackFrameIds);
            u.attackDuration = d.attackDuration;
            u.attackHitFrame = d.attackHitFrame;

            u.projectileSpeed = d.projectileSpeed;
            if (d.attackType == AttackType.Ranged && !string.IsNullOrEmpty(d.projectileFrameId))
                u.projectilePrefab = BuildProjectileTemplate(d.projectileFrameId);

            if (u.walkFrames.Length > 0) u.icon = u.walkFrames[0];
            return u;
        }

        /// <summary>Nạp unit theo id từ PlayerUnitStorage rồi dựng. Dùng cho unitLookup của thẻ.</summary>
        public static UnitDefinition BuildById(string unitId)
        {
            var d = PlayerUnitStorage.GetById(unitId);
            return d != null ? Build(d) : null;
        }

        private static Sprite[] LoadSprites(string[] ids)
        {
            if (ids == null) return new Sprite[0];
            var list = new List<Sprite>(ids.Length);
            foreach (var id in ids)
            {
                var s = CardArtStorage.Load(id);
                if (s != null) list.Add(s);
            }
            return list.ToArray();
        }

        // Tạo 1 GameObject đạn (ẩn) làm template — Unit.FireProjectile sẽ Instantiate clone nó.
        private static GameObject BuildProjectileTemplate(string spriteId)
        {
            var go = new GameObject("PlayerProjectile");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CardArtStorage.Load(spriteId);
            go.AddComponent<Projectile>();
            go.SetActive(false); // template; clone sẽ được SetActive(true) khi bắn
            return go;
        }
    }
}
