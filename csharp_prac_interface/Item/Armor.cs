using System;
using System.Collections.Generic;

namespace csharp_prac_interface
{
    // =========================================================================
    //  Armor: 장착 시 DEF가 증가하는 장비 아이템
    // =========================================================================
    public class Armor : Item, IDisassemblable
    {
        public int Defense { get; set; }
        public ArmorType ArmorType { get; }

        // 분해 / 비교 시 원본 이름 참조용
        public string BaseName { get; }

        // == 생성자 =============================================================
        internal Armor(string name, int weight, ItemRarity rarity,
                       int defense, ArmorType armorType, string baseName)
            : base(name, weight, rarity)
        {
            Defense = defense;
            ArmorType = armorType;
            BaseName = baseName;
        }

        // 하위 호환 / 수동 생성용 public 생성자
        public Armor(string name, int weight, ItemRarity rarity, int defense)
            : base(name, weight, rarity)
        {
            Defense = defense;
            ArmorType = ArmorType.Leather;
            BaseName = name;
        }

        // == UseItem ============================================================
        public override void UseItem()
        {
            NotifyItemUsed();
            Console.WriteLine($"  [장착] {Name}을(를) 장착합니다! 방어력 +{Defense}");
        }

        // == IDisassemblable ====================================================
        //  COMMON    → OldLeather ×1
        //  UNCOMMON  → OldLeather ×2
        //  RARE      → OldLeather ×3
        //  UNIQUE    → OldLeather ×4
        //  LEGENDARY → OldLeather ×5
        //  ANCIENT   → OldLeather ×5 + IronShard ×2
        public List<Material> Disassemble()
        {
            List<Material> result = new List<Material>();

            int leatherCount = Math.Min((int)Rarity + 1, 5);
            for (int i = 0; i < leatherCount; i++)
                result.Add(new OldLeather());

            if (Rarity == ItemRarity.ANCIENT)
            {
                result.Add(new IronShard());
                result.Add(new IronShard());
            }

            return result;
        }

        public override string ToString()
            => $"[{Rarity}] {Name}  DEF+{Defense}  (무게: {Weight})";
    }
}