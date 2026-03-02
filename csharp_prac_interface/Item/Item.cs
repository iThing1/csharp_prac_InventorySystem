using System;

namespace csharp_prac_interface
{
    public enum ItemRarity { COMMON, UNCOMMON, RARE, UNIQUE, LEGENDARY, ANCIENT }

    // == Item 추상 베이스 클래스 ===============================================
    //  모든 아이템의 공통 속성과 동작을 정의.
    //  Weapon / Armor / Potion / Material 이 이 클래스를 상속.
    // =========================================================================
    public abstract class Item
    {
        public string Name { get; set; }
        public int Weight { get; set; }
        public ItemRarity Rarity { get; private set; }

        protected Item(string name, int weight, ItemRarity rarity)
        {
            Name = name;
            Weight = weight;
            Rarity = rarity;
        }

        // 아이템마다 고유한 사용 효과
        public abstract void UseItem();

        // 사용 시 인벤토리 등 외부에 알리는 이벤트
        public event Action<Item> OnItemUsed;
        protected void NotifyItemUsed()
        {
            Console.WriteLine($"  ({Rarity}) {Name} 을(를) 사용했습니다!");
            OnItemUsed?.Invoke(this);
        }

        // 등급별 콘솔 출력 색상
        public ConsoleColor GetRarityColor()
        {
            switch (Rarity)
            {
                case ItemRarity.COMMON: return ConsoleColor.White;
                case ItemRarity.UNCOMMON: return ConsoleColor.Cyan;
                case ItemRarity.RARE: return ConsoleColor.Magenta;
                case ItemRarity.UNIQUE: return ConsoleColor.DarkYellow;
                case ItemRarity.LEGENDARY: return ConsoleColor.Yellow;
                case ItemRarity.ANCIENT: return ConsoleColor.Green;
                default: return ConsoleColor.Gray;
            }
        }
    }
}