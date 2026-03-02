using System;
using System.Collections.Generic;

namespace csharp_prac_interface
{
    public class Battle
    {
        private Player _player;
        private Monster _enemy;

        // IDropStrategy 주입: 외부에서 드롭 방식을 교체할 수 있습니다.
        // 미전달 시 DefaultDropStrategy 사용
        private IDropStrategy _dropStrategy;

        public Battle(Player player, Monster enemy, IDropStrategy dropStrategy = null)
        {
            _player = player;
            _enemy = enemy;
            _dropStrategy = dropStrategy ?? new DefaultDropStrategy();
        }

        // == 전투 진입점 =========================================================

        public List<Item> Run()
        {
            PrintBattleHeader();
            BattleLoop();
            return BattleEnd();
        }

        // == 전투 헤더 출력 ======================================================

        private void PrintBattleHeader()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n  ==============================");
            Console.WriteLine("              전  투              ");
            Console.WriteLine("  ==============================");
            Console.ResetColor();

            Console.WriteLine($"\n  [플레이어] {_player}");
            Console.WriteLine($"  [ 적 ] {_enemy}");
            Console.WriteLine();
        }

        // == 전투 루프 ===========================================================

        private void BattleLoop()
        {
            // Player와 Monster 모두 IDamageable을 구현하므로
            // 타입 구분 없이 동일한 방식으로 피격 처리할 수 있습니다.
            //
            // ★ TODO: 실제 전투 로직 구현 위치
            //
            // while (_player.IsAlive && _enemy.IsAlive)
            // {
            //     PlayerTurn();          // 플레이어 행동 선택 (공격 / 스킬 / 아이템 / 도망)
            //     if (!_enemy.IsAlive) break;
            //     EnemyTurn();           // 적 AI 행동
            // }
            //
            // IDamageable 활용 예시:
            //   private void PlayerAttack()
            //   {
            //       IDamageable target = _enemy;   // ← IDamageable로 참조
            //       target.TakeDamage(_player.Atk);
            //       Console.WriteLine($"  {_enemy.Name}에게 공격! (남은 HP: {target.Hp}/{target.MaxHp})");
            //   }
            //
            //   private void EnemyAttack()
            //   {
            //       IDamageable target = _player;  // ← 같은 인터페이스로 처리
            //       target.TakeDamage(_enemy.Atk);
            //   }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  [ 전투 로직 미구현 — BattleLoop() 내부를 채워주세요 ]");
            Console.ResetColor();
        }

        // == 전투 종료 처리 ======================================================

        private List<Item> BattleEnd()
        {
            // ★ BattleLoop() 구현 후 IsAlive(IDamageable)로 승패 분기
            // 현재는 테스트용으로 승리 처리만 실행
            bool playerWon = true;   // → _player.IsAlive 로 교체 예정

            if (playerWon)
                return OnVictory();
            else
            {
                OnDefeat();
                return new List<Item>();
            }
        }

        // == 승리 처리 ============================================================

        private List<Item> OnVictory()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  ★ 전투 승리! ★");
            Console.ResetColor();

            // 경험치 획득
            _player.GainExp(_enemy.Exp);

            // IDropStrategy에 드롭 계산 위임
            List<Item> drops = _dropStrategy.RollDrops(_enemy);

            if (drops.Count > 0)
            {
                Console.WriteLine($"\n  [ 아이템 드롭! - 총 {drops.Count}개 ]");

                // 재료(Material)는 같은 종류끼리 묶어서 "× N개" 형태로 출력
                Dictionary<string, int> matCount = new Dictionary<string, int>();
                Dictionary<string, Item> matSample = new Dictionary<string, Item>();

                foreach (Item dropped in drops)
                {
                    if (dropped is Material mat)
                    {
                        if (!matCount.ContainsKey(mat.Name)) { matCount[mat.Name] = 0; matSample[mat.Name] = mat; }
                        matCount[mat.Name]++;
                    }
                    else
                    {
                        Console.ForegroundColor = dropped.GetRarityColor();
                        Console.WriteLine($"    [{dropped.Rarity}] {dropped.Name}  (무게: {dropped.Weight})");
                        Console.ResetColor();
                    }
                }

                foreach (var kv in matCount)
                {
                    Item sample = matSample[kv.Key];
                    Console.ForegroundColor = sample.GetRarityColor();
                    Console.WriteLine($"    [{sample.Rarity}] {kv.Key}  × {kv.Value}개");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("\n  [ 드롭된 아이템이 없습니다. ]");
                Console.ResetColor();
            }

            return drops;
        }

        // == 패배 처리 ============================================================

        private void OnDefeat()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n  ✖ 전투 패배...");
            Console.ResetColor();

            // ★ TODO: 패배 후처리 (마을 귀환, HP 일부 회복, 골드 손실 등)
        }
    }
}