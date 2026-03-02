using System;
using System.Collections.Generic;

namespace csharp_prac_interface
{
    // =========================================================================
    //  드롭 전략 인터페이스
    // =========================================================================
    public interface IDropStrategy
    {
        List<Item> RollDrops(Monster enemy);
    }

    // =========================================================================
    //  기본 드롭 시스템 (DefaultDropStrategy)
    //
    //  ┌= 장비 드롭 (독립 롤, 횟수마다 확률 급감)
    //  │    1번째 : BASE_WEAPON_CHANCE  (40%)
    //  │    2번째 : 이전 확률 × WEAPON_DECAY (×0.20 = 8%)
    //  │    MIN_WEAPON_CHANCE (0.5%) 미만이면 중단
    //  │
    //  ├= 포션 드롭 (별도 독립 롤)
    //  │    1번째 : BASE_POTION_CHANCE  (60%)
    //  │    2번째 : 이전 확률 × POTION_DECAY (×0.35 = 21%)
    //  │    MIN_POTION_CHANCE (5%) 미만이면 중단
    //  │
    //  └= 재료 드롭 (독립 롤, 몬스터 그룹별 종류 결정)
    //       1번째 : BASE_MATERIAL_CHANCE (70%)
    //       2번째 : 이전 확률 × MATERIAL_DECAY (×0.40 = 28%)
    //       MIN_MATERIAL_CHANCE (5%) 미만이면 중단 / 정예 +15%
    //
    //  장비 등급 가중치 테이블 (레벨 구간별)
    //
    //  레벨  │ COM  UNC  RAR  UNI  LEG  ANC
    //  ======┼=============================
    //  1~ 3  │  55   30   10    4    1    0
    //  4~ 6  │  35   35   20    8    2    0
    //  7~ 9  │  20   30   30   15    4    1
    //  10+   │  10   20   30   25   12    3
    //
    //  정예 보너스: RARE+5 / UNIQUE+5 / LEGENDARY+4 / ANCIENT+1
    // =========================================================================
    public class DefaultDropStrategy : IDropStrategy
    {
        private readonly Random _rng = new Random();

        private const float BASE_WEAPON_CHANCE = 0.40f;
        private const float WEAPON_DECAY = 0.20f;
        private const float MIN_WEAPON_CHANCE = 0.005f;

        private const float BASE_POTION_CHANCE = 0.60f;
        private const float POTION_DECAY = 0.35f;
        private const float MIN_POTION_CHANCE = 0.05f;

        private static readonly int[,] ItemDropTable =
        {
            //   COM  UNC  RAR  UNI  LEG  ANC
            {     55,  30,  10,   4,   1,   0  },  // Tier 1 : Lv  1~3
            {     35,  35,  20,   8,   2,   0  },  // Tier 2 : Lv  4~6
            {     20,  30,  30,  15,   4,   1  },  // Tier 3 : Lv  7~9
            {     10,  20,  30,  25,  12,   3  },  // Tier 4 : Lv 10+
        };

        private static readonly int[] EliteBonus = { 0, 0, 5, 5, 4, 1 };

        private const float BASE_MATERIAL_CHANCE = 0.70f;
        private const float MATERIAL_DECAY = 0.40f;
        private const float MIN_MATERIAL_CHANCE = 0.05f;
        private const float ELITE_MATERIAL_BONUS = 0.15f;

        // == 진입점 =============================================================
        public List<Item> RollDrops(Monster enemy)
        {
            List<Item> drops = new List<Item>();

            // 장비 드롭 롤
            float weaponChance = BASE_WEAPON_CHANCE;
            while (weaponChance >= MIN_WEAPON_CHANCE)
            {
                if (_rng.NextDouble() >= weaponChance) break;
                drops.Add(RollEquipment(_rng, enemy.Level, enemy.Grade));
                weaponChance *= WEAPON_DECAY;
            }

            // 포션 드롭 롤
            float potionChance = BASE_POTION_CHANCE;
            while (potionChance >= MIN_POTION_CHANCE)
            {
                if (_rng.NextDouble() >= potionChance) break;
                drops.Add(RollPotion(_rng, enemy.Level));
                potionChance *= POTION_DECAY;
            }

            // 재료 드롭 롤
            drops.AddRange(RollMaterialDrops(_rng, enemy));

            return drops;
        }

        // == 장비 한 개 생성 (ItemTable 사용) =================================
        private Item RollEquipment(Random rng, int level, MonsterGrade grade)
        {
            ItemRarity rarity = RollRarity(rng, level, grade);

            // 무기 / 방어구 50:50
            if (rng.Next(2) == 0)
                return ItemTable.SpawnRandomWeapon(rarity);
            else
                return ItemTable.SpawnRandomArmor(rarity);
        }

        // == 등급 추첨 ==========================================================
        private static ItemRarity RollRarity(Random rng, int level, MonsterGrade grade)
        {
            int tier = LevelToTier(level);

            int[] weights = new int[6];
            for (int i = 0; i < 6; i++)
            {
                weights[i] = ItemDropTable[tier, i];
                if (grade == MonsterGrade.Elite)
                    weights[i] += EliteBonus[i];
            }

            int total = 0;
            foreach (int w in weights) total += w;

            int roll = rng.Next(total);
            int cumulative = 0;
            int rarityIdx = 0;

            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (roll < cumulative) { rarityIdx = i; break; }
            }

            return (ItemRarity)rarityIdx;
        }

        // == 포션 한 개 생성 ====================================================
        private static Item RollPotion(Random rng, int level)
        {
            int tier = LevelToTier(level);
            bool isHp = rng.Next(2) == 0;

            switch (tier)
            {
                case 0: return isHp ? (Item)new SmallHpPotion() : new SmallMpPotion();
                case 1: return isHp ? (Item)new MediumHpPotion() : new MediumMpPotion();
                case 2: return isHp ? (Item)new LargeHpPotion() : new LargeMpPotion();
                default: return isHp ? (Item)new XLargeHpPotion() : new XLargeMpPotion();
            }
        }

        // == 레벨 → 티어 ========================================================
        private static int LevelToTier(int level)
        {
            if (level <= 3) return 0;
            if (level <= 6) return 1;
            if (level <= 9) return 2;
            return 3;
        }

        // == 재료 드롭 롤 ========================================================
        private List<Item> RollMaterialDrops(Random rng, Monster enemy)
        {
            List<Item> result = new List<Item>();
            Material template = GetMaterialTemplate(enemy.Group);
            if (template == null) return result;

            float chance = BASE_MATERIAL_CHANCE
                         + (enemy.Grade == MonsterGrade.Elite ? ELITE_MATERIAL_BONUS : 0f);

            while (chance >= MIN_MATERIAL_CHANCE)
            {
                if (rng.NextDouble() >= chance) break;
                result.Add(GetMaterialTemplate(enemy.Group));
                chance *= MATERIAL_DECAY;
            }

            return result;
        }

        // == 몬스터 그룹 → 재료 매핑 =============================================
        private static Material GetMaterialTemplate(MonsterGroup group)
        {
            switch (group)
            {
                case MonsterGroup.Bandit:
                case MonsterGroup.TownMilitia:
                    return new OldLeather();

                case MonsterGroup.KnightOrder:
                case MonsterGroup.KnightSquire:
                    return new IronShard();

                default:
                    return null;
            }
        }
    }

    // =========================================================================
    //  드롭 없음 전략
    // =========================================================================
    public class NoDropStrategy : IDropStrategy
    {
        public List<Item> RollDrops(Monster enemy) => new List<Item>();
    }
}