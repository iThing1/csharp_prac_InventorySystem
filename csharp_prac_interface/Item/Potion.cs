using System;

namespace csharp_prac_interface
{
    // == Potion ===============================================================
    //  HP/MP 회복 포션의 공통 베이스.
    //  등급은 COMMON으로 고정.
    // =========================================================================
    public abstract class Potion : Item
    {
        public int Amount { get; protected set; }   // 회복량

        protected Potion(string name, int weight)
            : base(name, weight, ItemRarity.COMMON) { }

        public abstract void ApplyEffect(Player player);    // 포션효과 플레이어에게 적용
    }

    // == HpPotion =============================================================
    public abstract class HpPotion : Potion
    {
        protected HpPotion(string name, int weight, int heal)
            : base(name, weight) { Amount = heal; }

        public override void UseItem()
        {
            NotifyItemUsed();
            Console.WriteLine($"  HP를 {Amount} 회복합니다.");          
        }
        public override void ApplyEffect(Player player) => player.RestoreHp(Amount);
    }

    // == MpPotion =============================================================
    public abstract class MpPotion : Potion
    {
        protected MpPotion(string name, int weight, int mana)
            : base(name, weight) { Amount = mana; }

        public override void UseItem()
        {
            NotifyItemUsed();
            Console.WriteLine($"{Name} 을 사용하였습니다.");
            Console.WriteLine($"  MP를 {Amount} 회복합니다.");
        }
        public override void ApplyEffect(Player player) => player.RestoreMp(Amount);  
    }

    // == HP 포션 크기별 구현 ==================================================
    public class SmallHpPotion : HpPotion { public SmallHpPotion() : base("소형 회복 포션", weight: 1, heal: 30) { } }
    public class MediumHpPotion : HpPotion { public MediumHpPotion() : base("중형 회복 포션", weight: 3, heal: 70) { } }
    public class LargeHpPotion : HpPotion { public LargeHpPotion() : base("대형 회복 포션", weight: 4, heal: 130) { } }
    public class XLargeHpPotion : HpPotion { public XLargeHpPotion() : base("초대형 회복 포션", weight: 5, heal: 220) { } }

    // == MP 포션 크기별 구현 ==================================================
    public class SmallMpPotion : MpPotion { public SmallMpPotion() : base("소형 마나 포션", weight: 1, mana: 30) { } }
    public class MediumMpPotion : MpPotion { public MediumMpPotion() : base("중형 마나 포션", weight: 3, mana: 70) { } }
    public class LargeMpPotion : MpPotion { public LargeMpPotion() : base("대형 마나 포션", weight: 4, mana: 130) { } }
    public class XLargeMpPotion : MpPotion { public XLargeMpPotion() : base("초대형 마나 포션", weight: 5, mana: 220) { } }
}