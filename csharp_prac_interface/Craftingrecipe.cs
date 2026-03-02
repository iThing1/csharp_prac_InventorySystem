using System;
using System.Collections.Generic;

namespace csharp_prac_interface
{
    // =========================================================================
    //  Ingredient: 레시피 재료 한 종류 (재료 타입 + 필요 수량)
    // =========================================================================
    public class Ingredient
    {
        public Type MaterialType { get; }   // typeof(OldLeather) 등
        public int Amount { get; }
        public string DisplayName { get; }  // 인벤토리 출력용 이름

        public Ingredient(Type materialType, int amount, string displayName)
        {
            MaterialType = materialType;
            Amount = amount;
            DisplayName = displayName;
        }
    }

    // =========================================================================
    //  CraftingRecipe: 레시피 한 건 (재료 목록 + 결과 아이템 생성 함수)
    //
    //  새 레시피 추가 방법:
    //    CraftingTable._recipes 리스트에 한 줄 추가.
    //    다른 파일 수정 불필요.
    // =========================================================================
    public class CraftingRecipe
    {
        public string Name { get; }         // 레시피 이름 (UI 표시)
        public List<Ingredient> Ingredients { get; }
        public bool IsFinalGoal { get; }        // true → 합성 시 게임 클리어
        private readonly Func<Item> _factory;                 // 결과 아이템 생성 함수

        public CraftingRecipe(string name, List<Ingredient> ingredients,
                              Func<Item> factory, bool isFinalGoal = false)
        {
            Name = name;
            Ingredients = ingredients;
            _factory = factory;
            IsFinalGoal = isFinalGoal;
        }

        public Item Craft() => _factory();
    }

    // =========================================================================
    //  CraftingTable — 레시피 목록 + 합성 실행 로직
    // =========================================================================
    public static class CraftingTable
    {
        // == 레시피 목록 =========================================================
        //  새 레시피: 아래 리스트에 추가.
        public static readonly List<CraftingRecipe> Recipes = new List<CraftingRecipe>
        {
            // ★ 최종 목표 레시피: 고대의 용사검
            //   낡은 가죽 ×30 + 철 조각 ×20 → 고대의 용사검
            new CraftingRecipe(
                name: "전설의 용사검",
                ingredients: new List<Ingredient>
                {
                    new Ingredient(typeof(OldLeather), 30, "낡은 가죽"),
                    new Ingredient(typeof(IronShard),  20, "철 조각"),
                },
                factory: () => ItemTable.SpawnWeapon(WeaponType.Sword, ItemRarity.LEGENDARY),
                isFinalGoal: true
            ),
        };

        // == 재료 충족 여부 확인 =================================================

        public static bool CanCraft(CraftingRecipe recipe, Inventory inventory,
                                    out List<string> shortfall)
        {
            shortfall = new List<string>();

            foreach (Ingredient ing in recipe.Ingredients)
            {
                int have = inventory.CountMaterial(ing.MaterialType);
                if (have < ing.Amount)
                    shortfall.Add($"{ing.DisplayName}  {have} / {ing.Amount}");
            }

            return shortfall.Count == 0;
        }

        public static Item ExecuteCraft(CraftingRecipe recipe, Inventory inventory)
        {
            // 재료 차감
            foreach (Ingredient ing in recipe.Ingredients)
                inventory.ConsumeMaterial(ing.MaterialType, ing.Amount);

            return recipe.Craft();
        }
    }
}