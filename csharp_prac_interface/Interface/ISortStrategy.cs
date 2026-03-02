using System.Collections.Generic;
using System.Linq;

namespace csharp_prac_interface
{
    // 정렬 전략 인터페이스
    // 새로운 정렬 기준이 생겨도 이 인터페이스만 구현하면 됨(Inventory 건들 필요 없음)
    public interface ISortStrategy
    {
        string SortName { get; }
        List<Item> Sort(List<Item> items);
    }

    // 등급 오름차순 정렬 (COMMON -> ANCIENT)
    public class RaritySorter : ISortStrategy
    {
        public string SortName => "등급 오름차순";
        public List<Item> Sort(List<Item> items)
            => items.OrderBy(i => i.Rarity).ToList();
    }

    // 등급 내림차순 정렬 (ANCIENT -> COMMON)
    public class RarityDescSorter : ISortStrategy
    {
        public string SortName => "등급 내림차순";
        public List<Item> Sort(List<Item> items)
            => items.OrderByDescending(i => i.Rarity).ToList();
    }

    // 무게 오름차순 정렬
    public class WeightSorter : ISortStrategy
    {
        public string SortName => "무게 오름차순";
        public List<Item> Sort(List<Item> items)
            => items.OrderBy(i => i.Weight).ToList();
    }

    // 무게 내림차순 정렬
    public class WeightDescSorter : ISortStrategy
    {
        public string SortName => "무게 내림차순";
        public List<Item> Sort(List<Item> items)
            => items.OrderByDescending(i => i.Weight).ToList();
    }
}
