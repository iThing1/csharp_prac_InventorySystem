namespace csharp_prac_interface
{
    // 인벤토리 한 슬롯에 여러 개를 쌓을 수 있는 아이템이 구현하는 인터페이스.
    // Material 계열 아이템을 구현.

    public interface IStackable
    {
        // 현재 스택 수
        int StackCount { get; }

        // 슬롯 하나에 쌓을 수 있는 최대 수량
        int MaxStack { get; }

        // 스택이 꽉 찼는지 여부
        bool IsFull { get; }

        // 이 슬롯에 amount만큼 추가.
        int AddStack(int amount);
    }
}