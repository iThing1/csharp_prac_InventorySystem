namespace csharp_prac_interface
{
    // 무기 종류
    public enum WeaponType { Sword, Spear }

    // 방어구 종류
    public enum ArmorType { Cloth, Leather, Chain, Plate }

    // =========================================================================
    //  WeaponData — ItemTable에 등록되는 무기 원본 데이터
    //  Weapon.Create() 가 이 데이터를 참고해 실제 인스턴스를 생성.
    // =========================================================================
    public class WeaponData
    {
        public string BaseName { get; }
        public WeaponType WeaponType { get; }
        public int BaseDamage { get; }   // COMMON 기준 기본 데미지
        public int BaseWeight { get; }   // COMMON 기준 기본 무게

        public WeaponData(string baseName, WeaponType weaponType, int baseDamage, int baseWeight)
        {
            BaseName = baseName;
            WeaponType = weaponType;
            BaseDamage = baseDamage;
            BaseWeight = baseWeight;
        }
    }

    // =========================================================================
    //  ArmorData — ItemTable에 등록되는 방어구 원본 데이터
    // =========================================================================
    public class ArmorData
    {
        public string BaseName { get; }
        public ArmorType ArmorType { get; }
        public int BaseDefense { get; }
        public int BaseWeight { get; }

        public ArmorData(string baseName, ArmorType armorType, int baseDefense, int baseWeight)
        {
            BaseName = baseName;
            ArmorType = armorType;
            BaseDefense = baseDefense;
            BaseWeight = baseWeight;
        }
    }
}