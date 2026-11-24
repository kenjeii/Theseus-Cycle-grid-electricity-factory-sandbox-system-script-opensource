namespace TheseusCore.Grid
{
    /// <summary>
    /// Pure C# crafting evaluator. Checks affordability and consumes ingredients through
    /// <see cref="IInventory"/>; the caller decides what to instantiate from the result prefab.
    /// </summary>
    public sealed class CraftingService
    {
        private readonly IInventory _inventory;

        /// <summary>Creates a service over an inventory.</summary>
        public CraftingService(IInventory inventory)
        {
            _inventory = inventory;
        }

        /// <summary>Returns true when every ingredient is available in the required amount.</summary>
        public bool CanCraft(ICraftingRecipe recipe)
        {
            if (recipe == null) return false;

            var ingredients = recipe.RequiredIngredients;
            for (int i = 0; i < ingredients.Count; i++)
            {
                CraftingIngredient ingredient = ingredients[i];
                if (_inventory.Count(ingredient.materialId) < ingredient.amount) return false;
            }
            return true;
        }

        /// <summary>Consumes ingredients when affordable.</summary>
        public bool TryConsume(ICraftingRecipe recipe)
        {
            if (!CanCraft(recipe)) return false;

            var ingredients = recipe.RequiredIngredients;
            for (int i = 0; i < ingredients.Count; i++)
            {
                CraftingIngredient ingredient = ingredients[i];
                if (!_inventory.Remove(ingredient.materialId, ingredient.amount)) return false;
            }
            return true;
        }
    }

    /// <summary>Read-only view of a recipe, decoupled from ScriptableObject.</summary>
    public interface ICraftingRecipe
    {
        /// <summary>Ingredients required for one craft.</summary>
        System.Collections.Generic.IReadOnlyList<CraftingIngredient> RequiredIngredients { get; }
    }
}
