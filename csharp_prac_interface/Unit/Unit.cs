namespace csharp_prac_interface
{
    // 전투 유닛 공용 열거형
    public enum AttackType { NONE, MELEE, RANGE }
    public enum UnitType { Spear, Archer, Cavalry, Shield }

    // == Unit 추상 베이스 클래스 ================================================
    //  Player와 Monster의 공통 스탯을 정의.
    // ===================================================================
    public abstract class Unit
    {
        public UnitType UnitType { get; protected set; }
        public AttackType AttackType { get; protected set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int Hp { get; set; }
        public int Mp { get; set; }
        public int MaxHp { get; set; }
        public int MaxMp { get; set; }
        public int Atk { get; set; }
        public int Def { get; set; }
        public int Exp { get; protected set; }
        public int MaxExp { get; protected set; }
    }
}