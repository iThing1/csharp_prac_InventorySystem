using System;

namespace csharp_prac_interface
{
    public enum MonsterGrade { Normal, Elite }
    public enum MonsterGroup { Bandit, KnightOrder, KnightSquire, TownMilitia }

    // == MonsterData ===========================================================
    //  MonsterTable에 등록되는 몬스터 원본 데이터.
    //  Monster.Create()가 이 데이터를 참고해 실제 인스턴스를 생성.
    // ======================================================================
    public class MonsterData
    {
        public MonsterGroup Group { get; private set; }
        public UnitType UnitType { get; private set; }
        public AttackType AttackType { get; private set; }
        public string BaseName { get; private set; }
        public bool HasElite { get; private set; }
        public int BaseLevel { get; private set; }
        public int BaseMaxHp { get; private set; }
        public int BaseMaxMp { get; private set; }
        public int BaseAtk { get; private set; }
        public int BaseDef { get; private set; }
        public int BaseExp { get; private set; }

        public MonsterData(
            MonsterGroup group,
            UnitType unitType,
            AttackType attackType,
            string baseName,
            bool hasElite,
            int baseLevel,
            int baseMaxHp,
            int baseMaxMp,
            int baseAtk,
            int baseDef,
            int baseExp)
        {
            Group = group;
            UnitType = unitType;
            AttackType = attackType;
            BaseName = baseName;
            HasElite = hasElite;
            BaseLevel = baseLevel;
            BaseMaxHp = baseMaxHp;
            BaseMaxMp = baseMaxMp;
            BaseAtk = baseAtk;
            BaseDef = baseDef;
            BaseExp = baseExp;
        }
    }

    // == Monster ================================================================
    //  Unit을 상속하고 IDamageable을 구현하는 전투 참가자.
    //  직접 생성 불가 - Monster.Create() 또는 MonsterTable.Spawn*() 사용.
    // =========================================================================
    public class Monster : Unit, IDamageable
    {
        public MonsterGrade Grade { get; private set; }
        public MonsterGroup Group { get; private set; }

        private Monster() { }

        // == IDamageable 구현 =======================================================
        public bool IsAlive => Hp > 0;

        public void TakeDamage(int damage)
        {
            int actual = Math.Max(1, damage - Def);
            Hp = Math.Max(0, Hp - actual);
        }

        //  MonsterTable에서만 호출한다.
        internal static Monster Create(MonsterData data, MonsterGrade grade)
        {
            float mult = (grade == MonsterGrade.Elite) ? 1.2f : 1.0f;
            float expMult = (grade == MonsterGrade.Elite) ? 1.5f : 1.0f;

            Monster m = new Monster
            {
                Grade = grade,
                Group = data.Group,
                UnitType = data.UnitType,
                AttackType = data.AttackType,
                Name = (grade == MonsterGrade.Elite) ? $"[정예] {data.BaseName}" : data.BaseName,
                Level = data.BaseLevel,
                MaxHp = (int)(data.BaseMaxHp * mult),
                MaxMp = (int)(data.BaseMaxMp * mult),
                Atk = (int)(data.BaseAtk * mult),
                Def = (int)(data.BaseDef * mult),
                Exp = (int)(data.BaseExp * expMult),
            };
            m.Hp = m.MaxHp;
            m.Mp = m.MaxMp;
            return m;
        }

        public override string ToString() =>
            $"[{Grade}] {Name} (Lv.{Level})" +
            $"\n  HP {Hp}/{MaxHp}  ATK {Atk}  DEF {Def}";
    }
}