using System;
using System.Collections.Generic;

namespace csharp_prac_interface
{
    // =========================================================================
    //  Weapon: 장착 시 ATK가 증가하는 장비 아이템
    // =========================================================================
    public class Weapon : Item, IDisassemblable
    {
        public int Damage { get; set; }
        public WeaponType WeaponType { get; }

        // 분해 / 비교 시 원본 이름 참조용 (예: "영웅의 철검" → "철검")
        public string BaseName { get; }

        // == 생성자 =============================================================
        // ItemTable.CreateWeapon() 이 호출하는 내부 생성자
        internal Weapon(string name, int weight, ItemRarity rarity,
                        int damage, WeaponType weaponType, string baseName)
            : base(name, weight, rarity)
        {
            Damage = damage;
            WeaponType = weaponType;
            BaseName = baseName;
        }

        // 하위 호환 / 수동 생성용 public 생성자
        public Weapon(string name, int weight, ItemRarity rarity, int damage)
            : base(name, weight, rarity)
        {
            Damage = damage;
            WeaponType = WeaponType.Sword;
            BaseName = name;
        }

        // == UseItem ============================================================
        public override void UseItem()
        {
            NotifyItemUsed();
            Console.WriteLine($"  [장착] {Name}을(를) 장착합니다! 데미지 +{Damage}");
        }

        // == IDisassemblable ====================================================
        //  COMMON    → IronShard ×1
        //  UNCOMMON  → IronShard ×2
        //  RARE      → IronShard ×3
        //  UNIQUE    → IronShard ×4
        //  LEGENDARY → IronShard ×5
        //  ANCIENT   → IronShard ×5 + OldLeather ×2
        public List<Material> Disassemble()
        {
            List<Material> result = new List<Material>();

            int ironCount = Math.Min((int)Rarity + 1, 5);
            for (int i = 0; i < ironCount; i++)
                result.Add(new IronShard());

            if (Rarity == ItemRarity.ANCIENT)
            {
                result.Add(new OldLeather());
                result.Add(new OldLeather());
            }

            return result;
        }
        public override string ToString()
            => $"[{Rarity}] {Name}  ATK+{Damage}  (무게: {Weight})";
    }
}