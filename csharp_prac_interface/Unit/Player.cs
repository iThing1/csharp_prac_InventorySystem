using System;

namespace csharp_prac_interface
{
    // ★ 임시 플레이어 클래스 -> 추후 확장 예정
    public class Player : Unit, IDamageable
    {
        // 전투 테스트에서 획득한 누적 경험치
        public int CurrentExp { get; private set; }
        // 장비슬롯 설정
        public Weapon EquippedWeapon { get; private set; }
        public Armor EquippedArmor { get; private set; }

        public Player(string name, int level = 1)
        {
            Name = name;
            Level = level;

            // 레벨에 비례한 기본 스텟 (임시 공식)
            MaxHp = 100 + (Level - 1) * 20;
            MaxMp = 50 + (Level - 1) * 10;
            Atk = 10 + (Level - 1) * 3;
            Def = 5 + (Level - 1) * 2;
            MaxExp = Level * 100;   // 레벨업 필요 경험치

            Hp = MaxHp;
            Mp = MaxMp;
            CurrentExp = 0;
        }

        // == 경험치 획득 및 레벨업 처리 =========================================

        public void GainExp(int amount)
        {
            CurrentExp += amount;
            Console.WriteLine($"  EXP +{amount} ({CurrentExp} / {MaxExp})");

            while (CurrentExp >= MaxExp)
            {
                CurrentExp -= MaxExp;
                LevelUp();
            }
        }

        private void LevelUp()
        {
            Level++;
            MaxHp += 20;
            MaxMp += 10;
            Atk += 3;
            Def += 2;
            MaxExp = Level * 100;
            Hp = MaxHp;  // 레벨업 시 체력 전체 회복 (임시)
            Mp = MaxMp;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n  ★ LEVEL UP! Lv.{Level - 1} → Lv.{Level}");
            Console.ResetColor();
        }

        // == 전투 중 피격 ========================================================

        public void TakeDamage(int damage)
        {
            int actual = Math.Max(1, damage - Def);
            Hp = Math.Max(0, Hp - actual);
        }

        public bool IsAlive => Hp > 0;
        // 포션 회복(Max치 초과 불가)
        public void RestoreHp(int amount)
        {
            int before = Hp;
            Hp = Math.Min(MaxHp, Hp + amount);
            int actual = Hp - before;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  HP {before} → {Hp}  (+{actual})");
            Console.ResetColor();
        }
        public void RestoreMp(int amount)
        {
            int before = Mp;
            Mp = Math.Min(MaxMp, Mp + amount);
            int actual = Mp - before;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  MP {before} → {Mp}  (+{actual})");
            Console.ResetColor();
        }
        // 장비 장착
        // 이미 장착된 장비가 있을 경우, 교체 후 이전장비 반환
        public Weapon EquipWeapon(Weapon weapon)
        {
            Weapon prev = EquippedWeapon;

            // 이전 무기 ATK 제거
            if (prev != null)
                Atk -= prev.Damage;

            EquippedWeapon = weapon;
            Atk += weapon.Damage;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  [{weapon.Rarity}] {weapon.Name} 장착! (ATK +{weapon.Damage})");
            Console.ResetColor();

            if (prev != null)
                Console.WriteLine($"  (이전 무기 [{prev.Rarity}] {prev.Name} 해제)");

            return prev; // null이면 이전 무기 없음
        }
        // 장비 해제(해제 무기 반환)
        public Weapon UnequipWeapon()
        {
            if (EquippedWeapon == null)
            {
                Console.WriteLine("  장착된 무기가 없습니다.");
                return null;
            }

            Weapon prev = EquippedWeapon;
            Atk -= prev.Damage;
            EquippedWeapon = null;

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"  [{prev.Rarity}] {prev.Name} 해제 (ATK -{prev.Damage})");
            Console.ResetColor();

            return prev;
        }
        // 방어구 장착
        public Armor EquipArmor(Armor armor)
        {
            Armor prev = EquippedArmor;

            if (prev != null)
                Def -= prev.Defense;

            EquippedArmor = armor;
            Def += armor.Defense;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  [{armor.Rarity}] {armor.Name} 장착! (DEF +{armor.Defense})");
            Console.ResetColor();

            if (prev != null)
                Console.WriteLine($"  (이전 방어구 [{prev.Rarity}] {prev.Name} 해제)");

            return prev;
        }
        // 방어구 해제
        public Armor UnequipArmor()
        {
            if (EquippedArmor == null)
            {
                Console.WriteLine("  장착된 방어구가 없습니다.");
                return null;
            }

            Armor prev = EquippedArmor;
            Def -= prev.Defense;
            EquippedArmor = null;

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"  [{prev.Rarity}] {prev.Name} 해제 (DEF -{prev.Defense})");
            Console.ResetColor();

            return prev;
        }
        // == [DEBUG] 스텟 직접 설정 메서드 ======================================
        // 레벨업 공식을 우회해 스텟을 강제 설정(테스트 전용)

        public void DebugSetLevel(int level)
        {
            Level = level;
            MaxExp = level * 100;
            // 레벨 변경 시 스텟도 공식에 맞게 재계산
            MaxHp = 100 + (level - 1) * 20;
            MaxMp = 50 + (level - 1) * 10;
            Atk = 10 + (level - 1) * 3;
            Def = 5 + (level - 1) * 2;
            Hp = MaxHp;
            Mp = MaxMp;
        }

        public void DebugSetHp(int maxHp) { MaxHp = maxHp; Hp = maxHp; }
        public void DebugSetMp(int maxMp) { MaxMp = maxMp; Mp = maxMp; }
        public void DebugSetAtk(int atk) { Atk = atk; }
        public void DebugSetDef(int def) { Def = def; }
        public void DebugSetExp(int exp) { CurrentExp = exp; }

        public override string ToString() =>
            $"{Name} | Lv.{Level} | HP {Hp}/{MaxHp}  MP {Mp}/{MaxMp}  ATK {Atk}  DEF {Def}";
    }
}