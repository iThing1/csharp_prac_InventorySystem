using System;
using System.Collections.Generic;

namespace csharp_prac_interface
{
    // == MonsterTable ==========================================================
    //  몬스터 원본 데이터 테이블과 스폰 메서드를 관리.
    //
    //  새 몬스터 추가 방법:
    //   _table 리스트에 MonsterData 한 줄 추가.
    //   Monster 클래스나 다른 파일은 수정할 필요가 없다.
    // ======================================================================
    public static class MonsterTable
    {
        private static readonly List<MonsterData> _table = new List<MonsterData>
        {
            // Group                         UnitType            AttackType        BaseName          Elite   Lv   HP    MP  ATK  DEF  EXP
            new MonsterData(MonsterGroup.Bandit,       UnitType.Spear,   AttackType.MELEE, "도적 창병",      true,    2,  80,  20,  12,   6,  30),
            new MonsterData(MonsterGroup.Bandit,       UnitType.Archer,  AttackType.RANGE, "도적 궁수",      true,    3,  60,  30,  15,   4,  35),
            new MonsterData(MonsterGroup.Bandit,       UnitType.Cavalry, AttackType.MELEE, "도적 기병",      true,    5, 100,  20,  18,   8,  45),
            new MonsterData(MonsterGroup.Bandit,       UnitType.Shield,  AttackType.MELEE, "도적 방패병",    true,    4, 120,  10,  10,  14,  40),
            new MonsterData(MonsterGroup.KnightOrder,  UnitType.Cavalry, AttackType.MELEE, "기사단 기사",    true,   10, 150,  40,  22,  14,  80),
            new MonsterData(MonsterGroup.KnightSquire, UnitType.Shield,  AttackType.MELEE, "기사단 종자",    false,   7, 100,  20,  12,  18,  50),
            new MonsterData(MonsterGroup.TownMilitia,  UnitType.Spear,   AttackType.MELEE, "마을 자경단원",  false,   1,  70,  10,  10,   8,  20),
        };

        private static readonly Random _rng = new Random();

        // == 조회 ==============================================================
        public static List<MonsterData> GetByGroup(MonsterGroup group)
            => _table.FindAll(d => d.Group == group);

        public static MonsterData Get(MonsterGroup group, UnitType unitType)
        {
            MonsterData data = _table.Find(d => d.Group == group && d.UnitType == unitType);
            if (data == null)
                throw new KeyNotFoundException($"Monster not found: {group} / {unitType}");
            return data;
        }

        // == 스폰 ==============================================================

        // 그룹과 종류를 지정해 특정 몬스터를 스폰
        public static Monster Spawn(MonsterGroup group, UnitType unitType, MonsterGrade grade = MonsterGrade.Normal)
        {
            MonsterData data = Get(group, unitType);
            if (grade == MonsterGrade.Elite && !data.HasElite)
                throw new InvalidOperationException($"{data.BaseName}은(는) 정예 등급이 없습니다.");
            return Monster.Create(data, grade);
        }

        // 그룹 전체 몬스터를 Normal + Elite 포함해 모두 스폰
        public static List<Monster> SpawnGroup(MonsterGroup group)
        {
            List<Monster> result = new List<Monster>();
            foreach (MonsterData data in GetByGroup(group))
            {
                result.Add(Monster.Create(data, MonsterGrade.Normal));
                if (data.HasElite)
                    result.Add(Monster.Create(data, MonsterGrade.Elite));
            }
            return result;
        }

        // 플레이어 레벨 ±5 범위의 몬스터 중 랜덤으로 1마리 스폰.
        // 범위 내 몬스터가 없으면 가장 가까운 레벨로 대체한다.
        public static Monster SpawnForBattle(int playerLevel)
        {
            int min = playerLevel - 5;
            int max = playerLevel + 5;

            List<MonsterData> candidates = _table.FindAll(d => d.BaseLevel >= min && d.BaseLevel <= max);

            // 후보가 없으면 레벨 차이가 가장 작은 몬스터로 대체
            if (candidates.Count == 0)
            {
                MonsterData closest = _table[0];
                foreach (MonsterData d in _table)
                {
                    if (Math.Abs(d.BaseLevel - playerLevel) < Math.Abs(closest.BaseLevel - playerLevel))
                        closest = d;
                }
                candidates.Add(closest);
            }

            MonsterData selected = candidates[_rng.Next(candidates.Count)];

            // 플레이어보다 레벨이 3 이상 높으면 정예 등장 가능 (30% 확률)
            bool tryElite = selected.HasElite && (selected.BaseLevel - playerLevel >= 3);
            MonsterGrade grade = (tryElite && _rng.Next(100) < 30) ? MonsterGrade.Elite : MonsterGrade.Normal;

            return Monster.Create(selected, grade);
        }
    }
}