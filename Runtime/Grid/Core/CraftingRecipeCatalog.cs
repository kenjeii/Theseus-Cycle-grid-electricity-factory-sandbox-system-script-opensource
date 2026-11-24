using System.Collections.Generic;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Immutable code-only recipe definition. The source game stored recipes as
    /// ScriptableObjects whose only content-bound field was a prefab reference; here
    /// that reference is replaced with a stable catalog id.
    /// </summary>
    public sealed class RecipeDefinition : ICraftingRecipe
    {
        private readonly CraftingIngredient[] _ingredients;

        /// <summary>Unique recipe id.</summary>
        public string Id { get; }

        /// <summary>Catalog id of the produced placeable archetype.</summary>
        public string ResultId { get; }

        /// <summary>Number produced per craft.</summary>
        public int ResultCount { get; }

        /// <inheritdoc />
        public IReadOnlyList<CraftingIngredient> RequiredIngredients => _ingredients;

        /// <summary>Creates a recipe definition.</summary>
        public RecipeDefinition(string id, string resultId, int resultCount, params CraftingIngredient[] ingredients)
        {
            Id = id;
            ResultId = resultId;
            ResultCount = resultCount < 1 ? 1 : resultCount;
            _ingredients = ingredients ?? System.Array.Empty<CraftingIngredient>();
        }
    }

    /// <summary>
    /// All 32 crafting recipes from the source project, transcribed from the
    /// ScriptableObject assets under craft/recipe/demo+stage12. No assets are shipped.
    /// </summary>
    public static class CraftingRecipeCatalog
    {
        private static readonly RecipeDefinition[] AllRecipes =
        {
            new RecipeDefinition("block", "Block", 10,
                Ing("iron", 15)),

            new RecipeDefinition("blue_source", "Gg", 1,
                Ing("line", 2), Ing("iron", 2), Ing("glass", 2), Ing("blue_sol", 3)),

            new RecipeDefinition("bossrestrainer", "BbConstraint", 1,
                Ing("iron", 20), Ing("line", 20), Ing("titanium", 20), Ing("machinehand", 20)),

            new RecipeDefinition("braingenerator", "BossGen", 1,
                Ing("iron", 30), Ing("line", 30), Ing("titanium", 30), Ing("spinal_column", 1)),

            new RecipeDefinition("bullet_gen", "BulletGen", 1,
                Ing("alloy", 25), Ing("iron", 25), Ing("line", 25), Ing("circuit_board", 25)),

            new RecipeDefinition("bulletkillrecipe", "BulletKill", 1,
                Ing("line", 25), Ing("iron", 25), Ing("electromagetic_barrier", 15), Ing("circuit_board", 15)),

            new RecipeDefinition("cat_gen", "CatMachine", 1,
                Ing("line", 2), Ing("iron", 2), Ing("cat", 2), Ing("glass", 2)),

            new RecipeDefinition("drone", "CombatDroneStation", 1,
                Ing("ai_chip", 10), Ing("iron", 20), Ing("line", 15)),

            new RecipeDefinition("dummygenerator", "DummyGenerator", 1,
                Ing("line", 30), Ing("iron", 25), Ing("titanium", 50), Ing("man", 50),
                Ing("armor", 50), Ing("Energy_Reactor_Core", 50), Ing("circuit_board", 50)),

            new RecipeDefinition("electric_huge", "ElectricHuge", 1,
                Ing("iron", 20), Ing("line", 20), Ing("titanium", 20), Ing("alloy", 20),
                Ing("circuit_board", 20), Ing("gcrystal", 20), Ing("golden_fragment", 20)),

            new RecipeDefinition("fragment_generator", "Cm", 1,
                Ing("line", 30), Ing("iron", 25), Ing("titanium", 20), Ing("ggcore", 20),
                Ing("Energy_Reactor_Core", 20), Ing("circuit_board", 20), Ing("blackbord", 20)),

            new RecipeDefinition("green_source", "ElectricBig", 1,
                Ing("gcrystal", 25), Ing("line", 20), Ing("iron", 20)),

            new RecipeDefinition("gun", "GunMachine", 1,
                Ing("gun_core", 1), Ing("gun_machine", 1), Ing("iron", 40), Ing("line", 30),
                Ing("alloy", 20), Ing("circuit_board", 20)),

            new RecipeDefinition("laysertower", "LayerTower", 1,
                Ing("iron", 25), Ing("line", 25), Ing("prayer_bead", 1)),

            new RecipeDefinition("line", "Line2", 50,
                Ing("line", 5)),

            new RecipeDefinition("machinebase", "MachineBaseBaseS10", 1,
                Ing("line", 20), Ing("iron", 20), Ing("titanium", 20), Ing("ggcore", 20),
                Ing("Energy_Reactor_Core", 20), Ing("circuit_board", 50), Ing("blackbord", 50),
                Ing("swirl", 50), Ing("spring", 50), Ing("ovalcom", 50), Ing("machinehand", 50),
                Ing("gun_core", 50), Ing("gun_machine", 50), Ing("golden_fragment", 50),
                Ing("darkmachinecore", 50)),

            new RecipeDefinition("machinegun", "Gun1", 1,
                Ing("gun_machine", 15), Ing("iron", 25), Ing("line", 25), Ing("titanium", 25)),

            new RecipeDefinition("man", "FakeMan", 1,
                Ing("man", 3)),

            new RecipeDefinition("manbase", "Manbase", 1,
                Ing("line", 5), Ing("iron", 5), Ing("titanium", 5), Ing("alloy", 5), Ing("mancore", 5)),

            new RecipeDefinition("movemacine1", "MoveMachine1", 1,
                Ing("iron", 25), Ing("line", 25), Ing("titanium", 25), Ing("speedcore", 25), Ing("time_stone", 25)),

            // This recipe produces a prefab variant that does not carry PlaceableObject,
            // so its result id is intentionally outside PlaceableCatalog's 39 archetypes.
            new RecipeDefinition("movemacine2", "MoveMachine2Variant", 1,
                Ing("iron", 25), Ing("line", 25), Ing("titanium", 25), Ing("speedcore", 25), Ing("time_stone", 25)),

            new RecipeDefinition("punch", "Punch", 1,
                Ing("punch", 7), Ing("line", 5), Ing("iron", 5)),

            new RecipeDefinition("robotgenerator", "RobotMachine", 1,
                Ing("iron", 20), Ing("line", 20), Ing("titanium", 20), Ing("puchmachinecom", 20),
                Ing("ovalcom", 20), Ing("circuit_board", 20), Ing("alloy", 20), Ing("darkmachinecore", 20)),

            new RecipeDefinition("runner", "Runner", 1,
                Ing("swirl", 15), Ing("line", 15), Ing("iron", 15), Ing("spring", 15)),

            new RecipeDefinition("spawn1", "Spawn", 1,
                Ing("eyes", 5), Ing("line", 5), Ing("iron", 5)),

            new RecipeDefinition("spawn2", "Spawn2", 1,
                Ing("eyes", 30), Ing("line", 20), Ing("iron", 20), Ing("gcrystal", 20),
                Ing("alloy", 20), Ing("titanium", 20)),

            new RecipeDefinition("spawnpro", "SpawnPro", 1,
                Ing("iron", 20), Ing("line", 20), Ing("titanium", 20), Ing("blackbord", 20),
                Ing("alloy", 20), Ing("circuit_board", 20), Ing("ggcore", 20), Ing("gcrystal", 20)),

            new RecipeDefinition("step", "Step", 1,
                Ing("line", 3), Ing("iron", 3), Ing("spring", 4)),

            new RecipeDefinition("stink", "Stink", 1,
                Ing("spark", 2), Ing("line", 2), Ing("iron", 2)),

            new RecipeDefinition("teleport", "Teleport", 1,
                Ing("time_stone", 7), Ing("line", 5), Ing("iron", 5), Ing("glass", 5)),

            new RecipeDefinition("wallstep", "WallGen", 1,
                Ing("swirl", 8), Ing("line", 12), Ing("iron", 12), Ing("spring", 12), Ing("titanium", 12)),

            new RecipeDefinition("windmachine", "Windmac", 1,
                Ing("titanium", 50), Ing("line", 30), Ing("iron", 30), Ing("wave_barrel", 30))
        };

        /// <summary>All 32 recipes in registration order.</summary>
        public static IReadOnlyList<RecipeDefinition> All => AllRecipes;

        /// <summary>Looks up a recipe by id.</summary>
        public static bool TryGet(string id, out RecipeDefinition recipe)
        {
            for (int i = 0; i < AllRecipes.Length; i++)
            {
                if (AllRecipes[i].Id != id) continue;
                recipe = AllRecipes[i];
                return true;
            }

            recipe = null;
            return false;
        }

        /// <summary>Finds the recipe that produces a given catalog id, if any.</summary>
        public static bool TryGetByResult(string resultId, out RecipeDefinition recipe)
        {
            for (int i = 0; i < AllRecipes.Length; i++)
            {
                if (AllRecipes[i].ResultId != resultId) continue;
                recipe = AllRecipes[i];
                return true;
            }

            recipe = null;
            return false;
        }

        private static CraftingIngredient Ing(string materialId, int amount)
        {
            return new CraftingIngredient(materialId, amount);
        }
    }
}
