using System;

namespace csharp_prac_interface
{
    // == Material =============================================================
    //  IStackable을 구현하는 재료 아이템.
    //
    //  • 인벤토리 한 슬롯에 최대 MaxStack(5)개까지
    //  • Weight = 단위 무게(unitWeight) × StackCount 로 자동 동기화.
    //  • 등급은 COMMON으로 고정.
    //
    //  새 재료 추가 방법:
    //    이 파일 하단에 Material을 상속하는 클래스를 추가하고
    //    IDropStrategy.GetMaterialTemplate() 새 케이스를 등록.
    // =========================================================================
    public abstract class Material : Item, IStackable
    {
        private readonly int _unitWeight;

        public int StackCount { get; private set; }
        public int MaxStack { get; } = 5;
        public bool IsFull => StackCount >= MaxStack;

        protected Material(string name, int unitWeight)
            : base(name, unitWeight, ItemRarity.COMMON)
        {
            _unitWeight = unitWeight;
            StackCount = 1;
            SyncWeight();
        }

        // == 스택 추가: 남은 잔량 반환 (0이면 전부 성공) =======================
        public int AddStack(int amount)
        {
            int toAdd = Math.Min(MaxStack - StackCount, amount);
            StackCount += toAdd;
            SyncWeight();
            return amount - toAdd;
        }

        // == 스택 차감 (합성 소모용) =============================================
        // amount개만큼 스택을 차감. StackCount가 0 미만이 되지 않도록 보호.
        /// Inventory.ConsumeMaterial() 이 호출합니다.
        // 
        public void ConsumeStack(int amount)
        {
            StackCount = Math.Max(0, StackCount - amount);
            SyncWeight();
        }

        // == 같은 종류인지 비교 (Inventory 스택 합산에 사용) =============================
        public bool IsSameType(Material other) => other.GetType() == GetType();

        public override void UseItem()
        {
            NotifyItemUsed();
            Console.WriteLine($"  [재료] {Name}은(는) 사용할 수 없습니다.");
        }

        private void SyncWeight() => Weight = _unitWeight * StackCount;
    }

    // == 재료 구현체 ===========================================================

    // 낡은 가죽: 단위 무게 1 / Bandit, TownMilitia 드롭
    public class OldLeather : Material
    {
        public OldLeather() : base("낡은 가죽", unitWeight: 1) { }
    }

    // 철 조각: 단위 무게 2 / KnightOrder, KnightSquire 드롭
    public class IronShard : Material
    {
        public IronShard() : base("철 조각", unitWeight: 2) { }
    }
}