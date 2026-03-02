using System;
using System.Collections.Generic;
using System.Linq;

namespace csharp_prac_interface
{
    public class Inventory
    {
        private List<Item> _items = new List<Item>();
        private const int MaxWeight = 100;

        public event Action<Item> OnItemAdded;
        public event Action<Item> OnItemDiscarded;
        public event Action<string> OnSorted;

        // === 아이템 추가 =====================================================
        public void AddItem(Item item)
        {
            if (item is Material incoming)
            {
                int remaining = 1;
                foreach (Item slot in _items)
                {
                    if (slot is Material existing && !existing.IsFull && existing.IsSameType(incoming))
                    {
                        remaining = existing.AddStack(remaining);
                        if (remaining == 0) break;
                    }
                }

                if (remaining > 0)
                {
                    CheckOverWeight(incoming);
                    _items.Add(incoming);
                }
                OnItemAdded?.Invoke(incoming);
                return;
            }

            CheckOverWeight(item);
            _items.Add(item);
            OnItemAdded?.Invoke(item);
        }

        private void CheckOverWeight(Item item)
        {
            if (_items.Sum(i => i.Weight) + item.Weight > MaxWeight)
            {
                Console.WriteLine("경고!! 최대 무게를 초과하는 아이템입니다.");
                Console.WriteLine("이동속도가 초과무게에 비례해 느려집니다.");
            }
        }

        // === 인벤토리 출력 ===================================================
        public void ShowInventory(Player player = null)
        {
            if (_items.Count == 0)
            {
                Console.WriteLine("인벤토리가 비어 있습니다.");
                return;
            }

            Console.WriteLine("========== 인벤토리 목록 ==========");
            for (int i = 0; i < _items.Count; i++)
            {
                Item item = _items[i];
                Console.ForegroundColor = item.GetRarityColor();
                Console.Write($"  [{i + 1}] [{item.Rarity}] {item.Name}");
                Console.ResetColor();

                if (item is Material mat)
                    Console.Write($"  [{mat.StackCount}/{mat.MaxStack}]");

                Console.Write($"  (무게: {item.Weight})");

                // 장착 중인 아이템 표시
                if (player != null)
                {
                    if (item == player.EquippedWeapon) Console.Write("  🗡[장착중]");
                    if (item == player.EquippedArmor) Console.Write("  🛡[장착중]");
                }

                Console.WriteLine();
            }

            int totalWeight = _items.Sum(i => i.Weight);
            bool isOverWeight = totalWeight > MaxWeight;

            Console.Write($"  총 {_items.Count}개 아이템 | 무게: ");
            if (isOverWeight) Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(totalWeight);
            Console.ResetColor();
            Console.WriteLine($" / {MaxWeight}");

            if (isOverWeight)
                Console.WriteLine("< 경고: 최대 무게를 초과했습니다! >");
        }

        // === 포션 사용 ====================================================
        // 인덱스의 아이템을 사용.
        // 포션이면 ApplyEffect()로 Player 스탯을 실제 변경하고 인벤토리에서 제거.
        // 장비(Weapon/Armor)는 EquipItem()을 사용

        public bool UseItem(int index, Player player)
        {
            if (!ValidIndex(index)) return false;

            Item item = _items[index];

            if (item is Potion potion)
            {
                potion.UseItem();           // 메시지 출력
                potion.ApplyEffect(player); // 실제 HP / MP 회복
                _items.RemoveAt(index);
                return true;
            }

            if (item is Weapon || item is Armor)
            {
                Console.WriteLine("  장비 아이템은 '장착' 메뉴를 이용하세요.");
                return false;
            }

            if (item is Material)
            {
                Console.WriteLine("  재료 아이템은 사용할 수 없습니다.");
                return false;
            }

            item.UseItem();
            return true;
        }

        // === 장비 장착 / 해제 =============================================
        // 인덱스의 장비를 장착.
        // 이미 같은 슬롯에 장비가 있으면 자동으로 교체(이전 장비는 인벤토리에 유지)
        public bool EquipItem(int index, Player player)
        {
            if (!ValidIndex(index)) return false;

            Item item = _items[index];

            if (item is Weapon weapon)
            {
                // 이미 장착 중인 무기면 해제
                if (item == player.EquippedWeapon)
                {
                    player.UnequipWeapon();
                    return true;
                }
                player.EquipWeapon(weapon);
                return true;
            }

            if (item is Armor armor)
            {
                if (item == player.EquippedArmor)
                {
                    player.UnequipArmor();
                    return true;
                }
                player.EquipArmor(armor);
                return true;
            }

            Console.WriteLine("  장착할 수 없는 아이템입니다.");
            return false;
        }

        // === 장비 분해 ====================================================
        // 인덱스의 장비를 분해.
        // 성공 시 해당 아이템을 인벤토리에서 제거하고 재료를 추가한 뒤 재료 목록을 반환.
        // 실패 시 빈 리스트를 반환.

        public List<Material> DisassembleItem(int index, Player player)
        {
            if (!ValidIndex(index)) return new List<Material>();

            Item item = _items[index];

            if (!(item is IDisassemblable disassemblable))
            {
                Console.WriteLine("  분해할 수 없는 아이템입니다. (포션/재료는 분해 불가)");
                return new List<Material>();
            }

            // 현재 장착 중인 장비는 분해 불가
            if (item == player.EquippedWeapon || item == player.EquippedArmor)
            {
                Console.WriteLine("  장착 중인 장비는 분해할 수 없습니다. 먼저 해제하세요.");
                return new List<Material>();
            }

            List<Material> materials = disassemblable.Disassemble();

            // 원본 아이템 제거
            _items.RemoveAt(index);
            OnItemDiscarded?.Invoke(item);

            // 재료 인벤토리에 추가
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"\n  [{item.Rarity}] {item.Name} 분해 완료!");
            Console.ResetColor();
            Console.WriteLine($"  획득 재료:");

            foreach (Material mat in materials)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"    - {mat.Name}");
                Console.ResetColor();
                AddItem(mat);
            }

            return materials;
        }

        // === 아이템 버리기 ===================================================
        public bool DiscardItem(int index)
        {
            if (!ValidIndex(index)) return false;
            Item target = _items[index];
            _items.RemoveAt(index);
            OnItemDiscarded?.Invoke(target);
            return true;
        }

        public int DiscardBelowRarity(ItemRarity rarity, bool preview = false)
        {
            List<Item> targets = _items.Where(i => i.Rarity <= rarity).ToList();

            if (preview)
            {
                foreach (Item item in targets)
                {
                    Console.ForegroundColor = item.GetRarityColor();
                    Console.Write($"    [{item.Rarity}] {item.Name}");
                    if (item is Material mat)
                        Console.Write($"  [{mat.StackCount}/{mat.MaxStack}]");
                    Console.WriteLine($"  (무게: {item.Weight})");
                    Console.ResetColor();
                }
                return targets.Count;
            }

            foreach (Item target in targets)
            {
                _items.Remove(target);
                OnItemDiscarded?.Invoke(target);
            }
            return targets.Count;
        }

        // === 정렬 ============================================================
        public void SortItems(ISortStrategy strategy)
        {
            _items = strategy.Sort(_items);
            OnSorted?.Invoke(strategy.SortName);
        }

        // === 등급 필터 출력 ==================================================
        public void ShowByRarity(ItemRarity rarity)
        {
            var filtered = _items.Where(i => i.Rarity == rarity).ToList();
            if (filtered.Count == 0) { Console.WriteLine($"{rarity} 등급 아이템이 없습니다."); return; }

            Console.WriteLine($"=== {rarity} 등급 아이템 목록 ===");
            foreach (var item in filtered)
            {
                Console.ForegroundColor = item.GetRarityColor();
                Console.WriteLine($"  - {item.Name} (무게: {item.Weight})");
                Console.ResetColor();
            }
        }

        // === 합성 지원 메서드 =================================================

        // 특정 재료 타입의 인벤토리 내 총 수량을 반환.
        // 여러 슬롯에 나뉘어 있어도 합산
        public int CountMaterial(Type materialType)
        {
            int total = 0;
            foreach (Item item in _items)
                if (item is Material mat && mat.GetType() == materialType)
                    total += mat.StackCount;
            return total;
        }

        // 특정 재료를 amount개 소모.
        // 스택이 비면 슬롯을 제거하고 다음 슬롯으로 이어서 차감.
        public void ConsumeMaterial(Type materialType, int amount)
        {
            int remaining = amount;

            // 뒤에서 지워야 인덱스 꼬임 없음
            for (int i = _items.Count - 1; i >= 0 && remaining > 0; i--)
            {
                if (!(_items[i] is Material mat) || mat.GetType() != materialType)
                    continue;

                int take = Math.Min(mat.StackCount, remaining);

                // Material에 직접 StackCount를 줄이는 공개 메서드가 없으므로
                // AddStack에 음수를 쓰는 대신 Consume 경로를 사용.
                mat.ConsumeStack(take);
                remaining -= take;

                if (mat.StackCount == 0)
                {
                    _items.RemoveAt(i);
                    OnItemDiscarded?.Invoke(mat);
                }
            }
        }

        // === 공통 유효성 검사 ================================================
        private bool ValidIndex(int index)
        {
            if (index >= 0 && index < _items.Count) return true;
            Console.WriteLine($"  유효하지 않은 인덱스입니다: {index + 1}");
            return false;
        }
    }
}