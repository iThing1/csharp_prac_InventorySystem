using System;
using System.Collections.Generic;

namespace csharp_prac_interface
{
    // =========================================================================
    //  ItemTable — 장비 원본 데이터 테이블 + 팩토리
    //
    //  새 장비 추가 방법:
    //    _weapons 또는 _armors 리스트에 한 줄 추가.
    //    Weapon / Armor 클래스는 수정할 필요가 없다.
    //
    //  등급별 스탯 보너스 (기본 스탯 + 랜덤 범위)
    //  ┌============┬==========================================┐
    //  │ 등급                 │ 스탯 보너스 범위       │ 이름 접두사  │
    //  ├============┼==========================================┤
    //  │ COMMON         │ + 0 ~ 1                   │ (없음)          │
    //  │ UNCOMMON     │ + 2 ~ 4                   │ 견고한         │
    //  │ RARE                │ + 5 ~ 9                   │ 강화된         │
    //  │ UNIQUE            │ +10 ~ 15                 │ 영웅의         │
    //  │ LEGENDARY       │ +16 ~ 22                 │ 전설의         │
    //  │ ANCIENT           │ +23 ~ 30                 │ 고대의         │
    //  └============┴==========================================┘
    //  무게: RARE 이상부터 등급마다 +2 (희귀할수록 더 강하고 묵직함)
    // ==================================================================
    public static class ItemTable
    {
        // == 무기 원본 데이터 ==================================================
        //                         BaseName   WeaponType          BaseDamage  BaseWeight
        private static readonly List<WeaponData> _weapons = new List<WeaponData>
        {
            new WeaponData("목검",   WeaponType.Sword,  8,  2),
            new WeaponData("철검",   WeaponType.Sword,  15, 4),
            new WeaponData("단창",   WeaponType.Spear,  12, 3),
            new WeaponData("장창",   WeaponType.Spear,  18, 5),
        };

        // == 방어구 원본 데이터 ================================================
        //                         BaseName       ArmorType           BaseDefense  BaseWeight
        private static readonly List<ArmorData> _armors = new List<ArmorData>
        {
            new ArmorData("천갑옷",      ArmorType.Cloth,   3,  1),
            new ArmorData("가죽갑옷",    ArmorType.Leather, 6,  3),
            new ArmorData("사슬갑옷",    ArmorType.Chain,   10, 6),
            new ArmorData("플레이트 아머", ArmorType.Plate,  15, 10),
        };

        private static readonly Random _rng = new Random();

        // == 등급별 스탯 보너스 범위 [min, max] ================================
        private static readonly int[,] RarityBonusRange =
        {
            //  min  max
            {    0,   1  },  // COMMON
            {    2,   4  },  // UNCOMMON
            {    5,   9  },  // RARE
            {   10,  15  },  // UNIQUE
            {   16,  22  },  // LEGENDARY
            {   23,  30  },  // ANCIENT
        };

        // 등급별 이름 접두사
        private static readonly string[] RarityPrefix =
        {
            "",      // COMMON     (접두사 없음)
            "견고한 ", // UNCOMMON
            "강화된 ", // RARE
            "영웅의 ", // UNIQUE
            "전설의 ", // LEGENDARY
            "고대의 ", // ANCIENT
        };

        // == 조회 =============================================================
        public static WeaponData GetWeapon(WeaponType type)
        {
            WeaponData data = _weapons.Find(w => w.WeaponType == type);
            if (data == null)
                throw new KeyNotFoundException($"WeaponData not found: {type}");
            return data;
        }

        public static ArmorData GetArmor(ArmorType type)
        {
            ArmorData data = _armors.Find(a => a.ArmorType == type);
            if (data == null)
                throw new KeyNotFoundException($"ArmorData not found: {type}");
            return data;
        }

        // == 특정 무기 + 등급 지정 =====================================
        public static Weapon SpawnWeapon(WeaponType type, ItemRarity rarity)
        {
            WeaponData data = GetWeapon(type);
            return CreateWeapon(data, rarity);
        }

        public static Armor SpawnArmor(ArmorType type, ItemRarity rarity)
        {
            ArmorData data = GetArmor(type);
            return CreateArmor(data, rarity);
        }

        // == 랜덤 스폰 ==================================================

        public static Weapon SpawnRandomWeapon(ItemRarity rarity)
        {
            WeaponData data = _weapons[_rng.Next(_weapons.Count)];
            return CreateWeapon(data, rarity);
        }

        public static Armor SpawnRandomArmor(ItemRarity rarity)
        {
            ArmorData data = _armors[_rng.Next(_armors.Count)];
            return CreateArmor(data, rarity);
        }

        // == 내부 생성 로직 =====================================================
        private static Weapon CreateWeapon(WeaponData data, ItemRarity rarity)
        {
            int bonus = RollBonus(rarity);
            int weight = data.BaseWeight + WeightBonus(rarity);
            string name = BuildName(RarityPrefix[(int)rarity], data.BaseName);

            return new Weapon(name, weight, rarity,
                              damage: data.BaseDamage + bonus,
                              weaponType: data.WeaponType,
                              baseName: data.BaseName);
        }

        private static Armor CreateArmor(ArmorData data, ItemRarity rarity)
        {
            int bonus = RollBonus(rarity);
            int weight = data.BaseWeight + WeightBonus(rarity);
            string name = BuildName(RarityPrefix[(int)rarity], data.BaseName);

            return new Armor(name, weight, rarity,
                             defense: data.BaseDefense + bonus,
                             armorType: data.ArmorType,
                             baseName: data.BaseName);
        }

        // == 헬퍼 ==============================================================
        // 등급에 맞는 랜덤 스탯 보너스를 반환
        private static int RollBonus(ItemRarity rarity)
        {
            int idx = (int)rarity;
            int min = RarityBonusRange[idx, 0];
            int max = RarityBonusRange[idx, 1];
            return _rng.Next(min, max + 1);
        }

        // RARE 이상부터 등급마다 무게 +2
        private static int WeightBonus(ItemRarity rarity)
        {
            int r = (int)rarity;
            return r >= (int)ItemRarity.RARE ? r - (int)ItemRarity.RARE + 2 : 0;
        }

        private static string BuildName(string prefix, string baseName)
            => string.IsNullOrEmpty(prefix) ? baseName : prefix + baseName;
    }
}