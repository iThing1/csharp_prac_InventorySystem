using System;
using System.Collections.Generic;

namespace csharp_prac_interface
{
    class GameManager
    {
        private Inventory _inventory = new Inventory();
        private string _playerName;
        private Player _player;
        private bool _isGameCleared = false;   // 게임 클리어 플래그

        public GameManager()
        {
            _inventory.OnItemAdded += item => PrintColored($"   [{item.Rarity}] {item.Name} 이(가) 인벤토리에 추가되었습니다.", ConsoleColor.Green);
            _inventory.OnItemDiscarded += item => PrintColored($"   {item.Name} 을(를) 버렸습니다. (무게 -{item.Weight})", ConsoleColor.DarkRed);
            _inventory.OnSorted += name => PrintColored($"   정렬 완료 [기준: {name}]", ConsoleColor.Cyan);
        }

        // == 시작점 ==============================================================

        public void Run()
        {
            Console.Clear();
            Console.Write("  플레이어 이름을 입력하세요: ");
            _playerName = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(_playerName)) _playerName = "모험가";

            _player = new Player(_playerName, level: 1);

            MainLoop();
        }

        // == 메인 화면 ============================================================

        private void MainLoop()
        {
            while (true)
            {
                Console.Clear();
                PrintColored($"\n  안녕하세요, {_playerName} 님!\n", ConsoleColor.White);

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"  {_player}");
                Console.ResetColor();
                Console.WriteLine();

                // == 최종 목표 재료 진행도 표시 ==================================
                PrintGoalProgress();

                Console.WriteLine("  ==========================");
                Console.WriteLine("          메인 메뉴          ");
                Console.WriteLine("  ==========================");
                Console.WriteLine("    [1]  인벤토리 열기       ");
                Console.WriteLine("    [2]  전투               ");
                Console.WriteLine("    [3]  아이템 합성        ");
                Console.WriteLine("    [9]  [DEBUG] 스텟 조작   ");
                Console.WriteLine("    [0]  게임 종료           ");
                Console.WriteLine("  ==========================");
                Console.Write("\n  선택: ");

                switch (Console.ReadLine()?.Trim())
                {
                    case "1": InventoryLoop(); break;
                    case "2": BattleMenu(); break;
                    case "3":
                        CraftingMenu();
                        if (_isGameCleared) return;  // 게임 클리어 → 루프 탈출
                        break;
                    case "9": DebugStatMenu(); break;
                    case "0": ExitGame(); return;
                    default: Warn("올바른 번호를 입력해주세요."); break;
                }
            }
        }

        // == 최종 목표 진행도 출력 (메인 화면 상단) =============================

        private void PrintGoalProgress()
        {
            // 최종 목표 레시피만 찾아서 진행도 표시
            CraftingRecipe goal = CraftingTable.Recipes.Find(r => r.IsFinalGoal);
            if (goal == null) return;

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"  ★ 목표: {goal.Name} 합성");
            Console.ResetColor();

            foreach (Ingredient ing in goal.Ingredients)
            {
                int have = _inventory.CountMaterial(ing.MaterialType);
                int need = ing.Amount;
                bool enough = have >= need;

                Console.ForegroundColor = enough ? ConsoleColor.Green : ConsoleColor.DarkGray;
                Console.WriteLine($"    {ing.DisplayName,-10} {have,3} / {need}  {(enough ? "O" : "")}");
                Console.ResetColor();
            }
            Console.WriteLine();
        }

        // == 전투 메뉴 ============================================================

        private void BattleMenu()
        {
            Console.Clear();
            Monster enemy = MonsterTable.SpawnForBattle(_player.Level);

            Console.WriteLine($"  등장한 적: {enemy}\n");
            Console.Write("  전투를 시작하시겠습니까? [Y / N]: ");
            if (Console.ReadLine()?.Trim().ToUpper() != "Y")
            {
                PrintColored("  전투를 취소했습니다.", ConsoleColor.DarkGray);
                Pause();
                return;
            }

            List<Item> drops = new Battle(_player, enemy).Run();
            foreach (Item drop in drops)
                _inventory.AddItem(drop);
            Pause();
        }

        // == 합성 메뉴 ==========================================================

        private void CraftingMenu()
        {
            Console.Clear();
            PrintColored("  ==============================", ConsoleColor.DarkYellow);
            PrintColored("           합  성  소            ", ConsoleColor.DarkYellow);
            PrintColored("  ==============================", ConsoleColor.DarkYellow);
            Console.WriteLine();

            List<CraftingRecipe> recipes = CraftingTable.Recipes;

            // 레시피 목록 출력
            for (int i = 0; i < recipes.Count; i++)
            {
                CraftingRecipe r = recipes[i];
                bool ok = CraftingTable.CanCraft(r, _inventory, out _);

                Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.DarkGray;
                Console.Write($"  [{i + 1}]  {r.Name}");
                if (r.IsFinalGoal) Console.Write("  ★ 최종 목표");
                Console.ResetColor();
                Console.WriteLine();

                // 재료 필요량 표시
                foreach (Ingredient ing in r.Ingredients)
                {
                    int have = _inventory.CountMaterial(ing.MaterialType);
                    bool enough = have >= ing.Amount;
                    Console.ForegroundColor = enough ? ConsoleColor.Green : ConsoleColor.DarkGray;
                    Console.WriteLine($"         {ing.DisplayName,-10}  {have} / {ing.Amount}  {(enough ? "O" : $"(부족: {ing.Amount - have}개)")}");
                    Console.ResetColor();
                }
                Console.WriteLine();
            }

            Console.WriteLine("  [0]  돌아가기");
            Console.Write("\n  합성할 레시피 번호: ");

            string input = Console.ReadLine()?.Trim();
            if (input == "0" || string.IsNullOrEmpty(input)) return;

            if (!int.TryParse(input, out int idx) || idx < 1 || idx > recipes.Count)
            { Warn("올바른 번호를 입력해주세요."); return; }

            CraftingRecipe selected = recipes[idx - 1];

            // 재료 충족 검사
            if (!CraftingTable.CanCraft(selected, _inventory, out List<string> shortfall))
            {
                PrintColored("\n  재료가 부족합니다:", ConsoleColor.Red);
                foreach (string s in shortfall)
                    PrintColored($"    {s}", ConsoleColor.DarkRed);
                Pause();
                return;
            }

            // 합성 확인
            Console.Write($"\n  [{selected.Name}] 을(를) 합성하시겠습니까? [Y / N]: ");
            if (Console.ReadLine()?.Trim().ToUpper() != "Y")
            { PrintColored("  취소했습니다.", ConsoleColor.DarkGray); Pause(); return; }

            // 합성 실행
            Item result = CraftingTable.ExecuteCraft(selected, _inventory);
            _inventory.AddItem(result);

            Console.WriteLine();
            PrintColored("  ==============================", ConsoleColor.Yellow);
            PrintColored($"  합성 성공!  [{result.Rarity}] {result.Name}", ConsoleColor.Yellow);
            PrintColored("  ==============================", ConsoleColor.Yellow);

            // 최종 목표 달성 → 게임 클리어
            if (selected.IsFinalGoal)
            {
                Pause();
                GameClear(result);
            }
            else
            {
                Pause();
            }
        }

        // == 게임 클리어 화면 ====================================================

        private void GameClear(Item finalItem)
        {
            _isGameCleared = true;

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("  ==================================");
            Console.WriteLine("                                    ");
            Console.WriteLine("          ★  G A M E  C L E A R  ★  ");
            Console.WriteLine("                                    ");
            Console.WriteLine("  ==================================");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  {_playerName} 은(는) 마침내");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  [{finalItem.Rarity}] {finalItem.Name} 을(를) 완성했습니다!");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"  최종 스텟  |  {_player}");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  수고하셨습니다. 게임을 종료합니다.");
            Console.ResetColor();

            Pause();
        }

        // == 인벤토리 화면 ========================================================
        private void InventoryLoop()
        {
            while (true)
            {
                Console.Clear();
                _inventory.ShowInventory(_player);

                Console.WriteLine();
                Console.WriteLine("  ==========================================");
                Console.WriteLine("               인벤토리 메뉴                ");
                Console.WriteLine("  ==========================================");
                Console.WriteLine("    [1]  아이템 추가                        ");
                Console.WriteLine("    [2]  아이템 버리기                      ");
                Console.WriteLine("    [3]  정렬                               ");
                Console.WriteLine("    [4]  등급 필터                          ");
                Console.WriteLine("    [5]  포션 사용                          ");
                Console.WriteLine("    [6]  장비 장착 / 해제                   ");
                Console.WriteLine("    [7]  장비 분해                          ");
                Console.WriteLine("    [0]  메인 화면으로                      ");
                Console.WriteLine("  ==========================================");
                Console.Write("\n  선택: ");

                switch (Console.ReadLine()?.Trim())
                {
                    case "1": AddItemMenu(); break;
                    case "2": DiscardItemMenu(); break;
                    case "3": SortMenu(); break;
                    case "4": FilterMenu(); break;
                    case "5": UsePotionMenu(); break;
                    case "6": EquipItemMenu(); break;
                    case "7": DisassembleMenu(); break;
                    case "0": return;
                    default: Warn("올바른 번호를 입력해주세요."); break;
                }
            }
        }

        // === 포션 사용 ========================================================

        private void UsePotionMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"  현재 상태 | HP {_player.Hp}/{_player.MaxHp}  MP {_player.Mp}/{_player.MaxMp}");
            Console.ResetColor();
            Console.WriteLine();

            _inventory.ShowInventory(_player);

            Console.Write("\n  사용할 포션 번호 (1번부터, 0=취소): ");
            if (!int.TryParse(Console.ReadLine(), out int num) || num == 0) return;

            _inventory.UseItem(num - 1, _player);
            Pause();
        }

        // === 장비 장착 / 해제 =================================================

        private void EquipItemMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"  현재 상태 | ATK {_player.Atk}  DEF {_player.Def}");
            Console.ResetColor();

            if (_player.EquippedWeapon != null)
                PrintColored($"   장착 무기: [{_player.EquippedWeapon.Rarity}] {_player.EquippedWeapon.Name}", ConsoleColor.Yellow);
            if (_player.EquippedArmor != null)
                PrintColored($"   장착 방어구: [{_player.EquippedArmor.Rarity}] {_player.EquippedArmor.Name}", ConsoleColor.Yellow);
            Console.WriteLine();

            _inventory.ShowInventory(_player);

            Console.Write("\n  장착/해제할 아이템 번호 (1번부터, 0=취소): ");
            if (!int.TryParse(Console.ReadLine(), out int num) || num == 0) return;

            _inventory.EquipItem(num - 1, _player);
            Pause();
        }

        // === 장비 분해 ========================================================
        private void DisassembleMenu()
        {
            Console.Clear();
            PrintColored("  [ 장비 분해 ]  포션/재료는 분해할 수 없습니다.", ConsoleColor.DarkYellow);
            Console.WriteLine();

            _inventory.ShowInventory(_player);

            Console.Write("\n  분해할 아이템 번호 (1번부터, 0=취소): ");
            if (!int.TryParse(Console.ReadLine(), out int num) || num == 0) return;

            Console.Write("  정말 분해하시겠습니까? [Y / N]: ");
            if (Console.ReadLine()?.Trim().ToUpper() != "Y")
            { PrintColored("  취소했습니다.", ConsoleColor.DarkGray); Pause(); return; }

            _inventory.DisassembleItem(num - 1, _player);
            Pause();
        }

        // == 아이템 추가 ==========================================================
        private void AddItemMenu()
        {
            Console.Clear();
            Console.WriteLine("  === 아이템 추가 ===\n");
            Console.WriteLine("  아이템 종류: [1] 포션  [2] 무기  [3] 방어구");
            Console.Write("  선택: ");
            string typeInput = Console.ReadLine()?.Trim();
            if (typeInput != "1" && typeInput != "2" && typeInput != "3") { Warn("취소되었습니다."); return; }

            Console.Write("  이름: ");
            string name = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(name)) { Warn("이름을 입력해야 합니다."); return; }

            Console.Write("  무게 (숫자): ");
            if (!int.TryParse(Console.ReadLine(), out int weight) || weight <= 0)
            { Warn("올바른 무게를 입력해주세요."); return; }

            Console.WriteLine("  등급: [0]COMMON [1]UNCOMMON [2]RARE [3]UNIQUE [4]LEGENDARY [5]ANCIENT");
            Console.Write("  선택: ");
            if (!int.TryParse(Console.ReadLine(), out int rarityIndex) || rarityIndex < 0 || rarityIndex > 5)
            { Warn("올바른 등급을 입력해주세요."); return; }
            ItemRarity rarity = (ItemRarity)rarityIndex;

            if (typeInput == "1")
            {
                Console.Write("  HP/MP [1=HP, 2=MP]: ");
                string pt = Console.ReadLine()?.Trim();
                Console.Write("  티어 [1=소형, 2=중형, 3=대형, 4=초대형]: ");
                if (!int.TryParse(Console.ReadLine(), out int tier) || tier < 1 || tier > 4)
                { Warn("올바른 티어를 입력해주세요."); return; }

                Item potion;
                switch (pt)
                {
                    case "1": // HP 포션 결정
                        switch (tier)
                        {
                            case 1: potion = new SmallHpPotion(); break;
                            case 2: potion = new MediumHpPotion(); break;
                            case 3: potion = new LargeHpPotion(); break;
                            default: potion = new XLargeHpPotion(); break;
                        }
                        break;

                    default: // MP 포션 결정 (pt가 "1"이 아닌 모든 경우)
                        switch (tier)
                        {
                            case 1: potion = new SmallMpPotion(); break;
                            case 2: potion = new MediumMpPotion(); break;
                            case 3: potion = new LargeMpPotion(); break;
                            default: potion = new XLargeMpPotion(); break;
                        }
                        break;
                }

                // 생성된 포션을 인벤토리에 추가
                _inventory.AddItem(potion);
            }
            else if (typeInput == "2")
            {
                Console.Write("  데미지 (숫자): ");
                if (!int.TryParse(Console.ReadLine(), out int dmg) || dmg <= 0)
                { Warn("올바른 데미지를 입력해주세요."); return; }
                _inventory.AddItem(new Weapon(name, weight, rarity, dmg));
            }
            else
            {
                Console.Write("  방어력 (숫자): ");
                if (!int.TryParse(Console.ReadLine(), out int def) || def <= 0)
                { Warn("올바른 방어력을 입력해주세요."); return; }
                _inventory.AddItem(new Armor(name, weight, rarity, def));
            }

            Pause();
        }

        // == 아이템 버리기 ========================================================

        private void DiscardItemMenu()
        {
            Console.Clear();
            _inventory.ShowInventory(_player);

            Console.WriteLine();
            Console.WriteLine("  ==============================");
            Console.WriteLine("    [1]  개별 버리기            ");
            Console.WriteLine("    [2]  등급 이하 일괄 버리기  ");
            Console.WriteLine("    [0]  취소                   ");
            Console.WriteLine("  ==============================");
            Console.Write("\n  선택: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": DiscardSingle(); break;
                case "2": DiscardBelowRarity(); break;
                case "0": return;
                default: Warn("올바른 번호를 입력해주세요."); break;
            }
        }

        private void DiscardSingle()
        {
            Console.Write("\n  버릴 아이템 번호 (1번부터): ");
            if (!int.TryParse(Console.ReadLine(), out int num))
            { Warn("숫자를 입력해주세요."); return; }
            _inventory.DiscardItem(num - 1);
            Pause();
        }

        private void DiscardBelowRarity()
        {
            Console.WriteLine();
            Console.WriteLine("  등급 선택 (해당 등급 포함 이하 전부 버립니다)");
            Console.WriteLine("  [0]COMMON [1]UNCOMMON [2]RARE [3]UNIQUE [4]LEGENDARY");
            Console.Write("\n  선택: ");

            if (!int.TryParse(Console.ReadLine(), out int idx) || idx < 0 || idx > 4)
            { Warn("올바른 등급을 입력해주세요."); return; }

            ItemRarity threshold = (ItemRarity)idx;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"\n  [ {threshold} 이하 등급 아이템 버리기 ]");
            Console.ResetColor();

            int count = _inventory.DiscardBelowRarity(threshold, preview: true);
            if (count == 0) { PrintColored("  해당 등급 이하 아이템이 없습니다.", ConsoleColor.DarkGray); Pause(); return; }

            Console.Write($"\n  위 {count}개 아이템을 버리겠습니까? [Y / N]: ");
            if (Console.ReadLine()?.Trim().ToUpper() != "Y") { PrintColored("  취소했습니다.", ConsoleColor.DarkGray); Pause(); return; }

            int discarded = _inventory.DiscardBelowRarity(threshold, preview: false);
            PrintColored($"  {discarded}개 아이템을 버렸습니다.", ConsoleColor.DarkRed);
            Pause();
        }

        // == 정렬 =================================================================

        private void SortMenu()
        {
            Console.Clear();
            Console.WriteLine("  === 정렬 기준 선택 ===\n");
            Console.WriteLine("  [1] 등급 오름차순   [2] 등급 내림차순");
            Console.WriteLine("  [3] 무게 오름차순   [4] 무게 내림차순");
            Console.Write("\n  선택: ");

            ISortStrategy strategy = null;
            switch (Console.ReadLine()?.Trim())
            {
                case "1": strategy = new RaritySorter(); break;
                case "2": strategy = new RarityDescSorter(); break;
                case "3": strategy = new WeightSorter(); break;
                case "4": strategy = new WeightDescSorter(); break;
            }

            if (strategy == null) { Warn("올바른 번호를 입력해주세요."); return; }
            _inventory.SortItems(strategy);
            Pause();
        }

        // == 등급 필터 ============================================================

        private void FilterMenu()
        {
            Console.Clear();
            Console.WriteLine("  === 등급 필터 ===\n");
            Console.WriteLine("  [0]COMMON [1]UNCOMMON [2]RARE [3]UNIQUE [4]LEGENDARY [5]ANCIENT");
            Console.Write("\n  선택: ");

            if (!int.TryParse(Console.ReadLine(), out int idx) || idx < 0 || idx > 5)
            { Warn("올바른 등급을 입력해주세요."); return; }

            Console.WriteLine();
            _inventory.ShowByRarity((ItemRarity)idx);
            Pause();
        }

        // == [DEBUG] 스텟 조작 ====================================================

        private void DebugStatMenu()
        {
            while (true)
            {
                Console.Clear();
                PrintColored("  [ DEBUG ] 스텟 조작", ConsoleColor.DarkYellow);
                Console.WriteLine($"\n  현재 스텟: {_player}\n");

                Console.WriteLine("  ======================================");
                Console.WriteLine("    [1]  레벨 설정                     ");
                Console.WriteLine("    [2]  HP 설정                       ");
                Console.WriteLine("    [3]  MP 설정                       ");
                Console.WriteLine("    [4]  ATK 설정                      ");
                Console.WriteLine("    [5]  DEF 설정                      ");
                Console.WriteLine("    [6]  경험치(CurrentExp) 설정       ");
                Console.WriteLine("    [7]  모든 스텟 한번에 설정          ");
                Console.WriteLine("    [8]  [DEBUG] 재료 즉시 지급         ");
                Console.WriteLine("    [0]  돌아가기                      ");
                Console.WriteLine("  ======================================");
                Console.Write("\n  선택: ");

                switch (Console.ReadLine()?.Trim())
                {
                    case "1": DebugSetStat("레벨", v => _player.DebugSetLevel(v)); break;
                    case "2": DebugSetStat("MaxHP", v => _player.DebugSetHp(v)); break;
                    case "3": DebugSetStat("MaxMP", v => _player.DebugSetMp(v)); break;
                    case "4": DebugSetStat("ATK", v => _player.DebugSetAtk(v)); break;
                    case "5": DebugSetStat("DEF", v => _player.DebugSetDef(v)); break;
                    case "6": DebugSetStat("EXP", v => _player.DebugSetExp(v)); break;
                    case "7": DebugSetAllStats(); break;
                    case "8": DebugGiveGoalMaterials(); break;
                    case "0": return;
                    default: Warn("올바른 번호를 입력해주세요."); break;
                }
            }
        }

        private void DebugSetStat(string label, Action<int> setter)
        {
            Console.Write($"  {label} 값 입력: ");
            if (!int.TryParse(Console.ReadLine(), out int val) || val <= 0)
            { Warn("1 이상의 숫자를 입력해주세요."); return; }
            setter(val);
            PrintColored($"  {label} → {val} 설정 완료", ConsoleColor.DarkYellow);
            Pause();
        }

        private void DebugSetAllStats()
        {
            Console.Clear();
            PrintColored("  [ DEBUG ] 전체 스텟 설정\n", ConsoleColor.DarkYellow);

            int level, hp, mp, atk, def;
            Console.Write("  레벨  : "); if (!int.TryParse(Console.ReadLine(), out level) || level <= 0) { Warn("취소되었습니다."); return; }
            Console.Write("  MaxHP : "); if (!int.TryParse(Console.ReadLine(), out hp) || hp <= 0) { Warn("취소되었습니다."); return; }
            Console.Write("  MaxMP : "); if (!int.TryParse(Console.ReadLine(), out mp) || mp <= 0) { Warn("취소되었습니다."); return; }
            Console.Write("  ATK   : "); if (!int.TryParse(Console.ReadLine(), out atk) || atk <= 0) { Warn("취소되었습니다."); return; }
            Console.Write("  DEF   : "); if (!int.TryParse(Console.ReadLine(), out def) || def <= 0) { Warn("취소되었습니다."); return; }

            _player.DebugSetLevel(level);
            _player.DebugSetHp(hp);
            _player.DebugSetMp(mp);
            _player.DebugSetAtk(atk);
            _player.DebugSetDef(def);

            PrintColored($"\n  전체 스텟 설정 완료", ConsoleColor.DarkYellow);
            Console.WriteLine($"  {_player}");
            Pause();
        }

        // [DEBUG] 최종 목표 재료 즉시 지급
        private void DebugGiveGoalMaterials()
        {
            CraftingRecipe goal = CraftingTable.Recipes.Find(r => r.IsFinalGoal);
            if (goal == null) { Warn("최종 목표 레시피가 없습니다."); return; }

            foreach (Ingredient ing in goal.Ingredients)
            {
                int have = _inventory.CountMaterial(ing.MaterialType);
                int toAdd = Math.Max(0, ing.Amount - have);

                for (int i = 0; i < toAdd; i++)
                {
                    Material mat = (Material)Activator.CreateInstance(ing.MaterialType);
                    _inventory.AddItem(mat);
                }
            }

            PrintColored("  [DEBUG] 최종 목표 재료를 지급했습니다.", ConsoleColor.DarkYellow);
            Pause();
        }

        // == 종료 =================================================================

        private void ExitGame()
        {
            Console.Clear();
            PrintColored($"\n  {_playerName} 님, 다음에 또 만나요!\n", ConsoleColor.Yellow);
            Pause();
        }

        // == 편의 메서드 ==========================================================
        private static void Warn(string msg)
        {
            PrintColored($"\n  {msg}", ConsoleColor.Red);
            Pause();
        }

        private static void Pause()
        {
            Console.Write("\n  [Enter] 계속...");
            Console.ReadLine();
        }

        private static void PrintColored(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}