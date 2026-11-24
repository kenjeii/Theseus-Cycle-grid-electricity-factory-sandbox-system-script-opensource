using System.Collections.Generic;
using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Code-only catalog of every placeable archetype in the source game. Values are
    /// transcribed from the 39 prefabs that carry the PlaceableObject component; no
    /// prefabs, sprites or other binary assets are shipped in this package.
    ///
    /// <para>
    /// The original game shipped these objects through Addressables/prefab references.
    /// Here each archetype is a plain value so it can be used for tests, tooling,
    /// procedural generation, or as the authoritative data source when building prefabs.
    /// </para>
    /// </summary>
    public static class PlaceableCatalog
    {
        // -------------------------------------------------------------------------
        // Boss / boss-related structures
        // -------------------------------------------------------------------------

        /// <summary>Boss-type generator placed during the third stage.</summary>
        public static readonly PlaceableDefinition Boss3Gen = Def(
            "Boss3Gen", "object_id", "", 36, 34, 0.49f, 0.47f, false, ObjectType.Consumer, -1);

        /// <summary>Brain King generator (large, overlap-capable).</summary>
        public static readonly PlaceableDefinition BossGen = Def(
            "BossGen", "boss_generator", "Brain King Biochemical Replicator", 78, 67, 0.48f, 0.67f, true, ObjectType.Consumer, 0);

        /// <summary>Constraint device that suppresses the Brain King.</summary>
        public static readonly PlaceableDefinition BbConstraint = Def(
            "BbConstraint", "bbcon", "Brain King Restraint Device", 107, 113, 0.5f, 0.44f, true, ObjectType.Consumer, 0);

        // -------------------------------------------------------------------------
        // Bullet systems
        // -------------------------------------------------------------------------

        /// <summary>Generator that harvests energy from destroyed bullets.</summary>
        public static readonly PlaceableDefinition BulletGen = Def(
            "BulletGen", "bullet_to_elect", "Bullet Generator", 119, 75, 0.47f, 0.5f, true, ObjectType.Generator, 0);

        /// <summary>Barrier that absorbs enemy projectiles.</summary>
        public static readonly PlaceableDefinition BulletKill = Def(
            "BulletKill", "bullet_kill", "Bullet-clearing barrier", 18, 15, 0.39f, 0.46f, true, ObjectType.Consumer, -12);

        // -------------------------------------------------------------------------
        // Machines
        // -------------------------------------------------------------------------

        /// <summary>Meow generator. Produces power passively.</summary>
        public static readonly PlaceableDefinition CatMachine = Def(
            "CatMachine", "catmachine", "Meow-Generator", 15, 7, 0.43f, 0.7f, true, ObjectType.Generator, 276);

        /// <summary>Monster fragment disintegration power generator.</summary>
        public static readonly PlaceableDefinition Cm = Def(
            "Cm", "capturemachine", "Monster Fragment Disintegration Power Generator", 77, 161, 0.38f, 0.52f, true, ObjectType.Generator, 0);

        /// <summary>Combat drone station.</summary>
        public static readonly PlaceableDefinition CombatDroneStation = Def(
            "CombatDroneStation", "combatdonre", "Drone", 14, 14, 0.45f, 0.55f, true, ObjectType.Consumer, -1);

        /// <summary>Drone station variant.</summary>
        public static readonly PlaceableDefinition DroneStation = Def(
            "DroneStation", "drone", "Drone", 19, 21, 0.46f, 0.59f, false, ObjectType.Consumer, -1);

        /// <summary>Electric dummy that generates power when attacked.</summary>
        public static readonly PlaceableDefinition DummyGenerator = Def(
            "DummyGenerator", "armur_gen", "Electric Dummy", 52, 58, 0.49f, 0.3f, true, ObjectType.Generator, 276);

        /// <summary>Medium power storage.</summary>
        public static readonly PlaceableDefinition ElectricBig = Def(
            "ElectricBig", "electric_source_green", "power source GREEN", 22, 48, 0.35f, 0.59f, true, ObjectType.Storage, 64);

        /// <summary>Large power storage.</summary>
        public static readonly PlaceableDefinition ElectricHuge = Def(
            "ElectricHuge", "electric_source_huge", "power source GOLD", 72, 59, 0.55f, 0.52f, true, ObjectType.Storage, 12);

        /// <summary>Decoy that attracts monster attacks.</summary>
        public static readonly PlaceableDefinition FakeMan = Def(
            "FakeMan", "fakeman", "Dummy", 2, 8, 0.49f, 0.62f, true, ObjectType.None, 276);

        /// <summary>Fearlight structure.</summary>
        public static readonly PlaceableDefinition FearLight = Def(
            "FearLight", "fearlight", "", 14, 27, 0.45f, 0.55f, false, ObjectType.Consumer, -1);

        /// <summary>Small power storage.</summary>
        public static readonly PlaceableDefinition Gg = Def(
            "Gg", "electric_source_blue", "power source BLUE", 17, 6, 0.5f, 0.5f, true, ObjectType.Storage, 64);

        /// <summary>Machine gun variant.</summary>
        public static readonly PlaceableDefinition Gun1 = Def(
            "Gun1", "gun1", "Machine Gun", 33, 10, 0.51f, 0.57f, true, ObjectType.Consumer, -12);

        /// <summary>Heavy gun that consumes large amounts of power.</summary>
        public static readonly PlaceableDefinition GunMachine = Def(
            "GunMachine", "gunmachine", "Energy Cannon", 63, 31, 0.66f, 0.57f, false, ObjectType.Consumer, -12);

        /// <summary>Laser tower.</summary>
        public static readonly PlaceableDefinition LayerTower = Def(
            "LayerTower", "laysertower", "laser tower", 19, 45, 0.41f, 0.52f, true, ObjectType.Consumer, -12);

        /// <summary>Electric wire tile (2x2).</summary>
        public static readonly PlaceableDefinition Line2 = Def(
            "Line2", "line", "Electric Wires", 2, 2, 0.56f, 0.55f, true, ObjectType.Wire, 10,
            allowContinuousDrag: true, continuousSpacingBySize: true);

        /// <summary>Electric wire tile (2x4) used as an alternate placement size.</summary>
        public static readonly PlaceableDefinition Line3 = Def(
            "Line3", "8", "Electric Wires", 2, 4, 0.56f, 0.52f, true, ObjectType.Wire, 10,
            allowContinuousDrag: true, continuousSpacingBySize: true);

        /// <summary>High-precision spaceship and battle armour manufacturing base.</summary>
        public static readonly PlaceableDefinition MachineBaseBaseS10 = Def(
            "MachineBaseBaseS10", "machinebase", "High-Precision Spaceship and Battle Armor Manufacturing Base", 96, 21, 0.52f, 0.13f, true, ObjectType.Consumer, 11);

        /// <summary>Light-speed travel device.</summary>
        public static readonly PlaceableDefinition MoveMachine1 = Def(
            "MoveMachine1", "movemachine1", "light-speed travel device", 45, 14, 0.51f, 0.51f, true, ObjectType.Consumer, -12);

        /// <summary>Punch machine.</summary>
        public static readonly PlaceableDefinition Punch = Def(
            "Punch", "punch", "punch machine", 20, 10, 0.5f, 0.45f, true, ObjectType.Consumer, -12);

        /// <summary>Unit Zero mass production device.</summary>
        public static readonly PlaceableDefinition RobotMachine = Def(
            "RobotMachine", "robotmachine", "Unit Zero Mass Production Device", 26, 34, 0.49f, 0.48f, true, ObjectType.Consumer, -1);

        /// <summary>Treadmill generator.</summary>
        public static readonly PlaceableDefinition Runner = Def(
            "Runner", "runner_gen", "treadmill generator", 28, 25, 0.5f, 0.48f, true, ObjectType.Generator, 0);

        /// <summary>Sealed machine variant.</summary>
        public static readonly PlaceableDefinition SealMachine = Def(
            "SealMachine", "sealmachine", "", 80, 76, 0.51f, 0.5f, true, ObjectType.Generator, 276);

        /// <summary>Spawn machine (three monster types).</summary>
        public static readonly PlaceableDefinition Spawn = Def(
            "Spawn", "spawn", "Spawn machine", 10, 3, 0.48f, 0.52f, true, ObjectType.Consumer, 0);

        /// <summary>Biochemical living device.</summary>
        public static readonly PlaceableDefinition Spawn2 = Def(
            "Spawn2", "spawn2", "Biochemical Living Device", 20, 35, 0.47f, 0.73f, true, ObjectType.Consumer, 0);

        /// <summary>Biological cloning transport station.</summary>
        public static readonly PlaceableDefinition SpawnPro = Def(
            "SpawnPro", "spawnpro", "Biological Cloning Transport Station", 113, 21, 0.53f, 0.51f, true, ObjectType.Consumer, 0);

        /// <summary>Step generator.</summary>
        public static readonly PlaceableDefinition Step = Def(
            "Step", "step_gen", "Step-Generator", 7, 3, 0.49f, 0.53f, true, ObjectType.Generator, 0);

        /// <summary>Spike machine.</summary>
        public static readonly PlaceableDefinition Stink = Def(
            "Stink", "stink", "spike machine", 16, 8, 0.26f, 0.41f, true, ObjectType.Consumer, -12);

        /// <summary>Teleport machine.</summary>
        public static readonly PlaceableDefinition Teleport = Def(
            "Teleport", "teleport", "Teleport Machine", 11, 25, 0.65f, 0.58f, true, ObjectType.Consumer, -12);

        /// <summary>Typhoon machine.</summary>
        public static readonly PlaceableDefinition Windmac = Def(
            "Windmac", "windmac", "typhoon machine", 36, 33, 0.51f, 0.51f, true, ObjectType.Consumer, -12);

        /// <summary>Man-base structure.</summary>
        public static readonly PlaceableDefinition Manbase = Def(
            "manbase", "unique_object_id", "", 48, 38, 0.5f, 0.39f, true, ObjectType.Consumer, -1);

        // -------------------------------------------------------------------------
        // Props and utility variants
        // -------------------------------------------------------------------------

        /// <summary>Monster-attached wire variant.</summary>
        public static readonly PlaceableDefinition Gp1 = Def(
            "Gp1", "line", "Electric Wires", 3, 4, 0.56f, 0.55f, true, ObjectType.Wire, 10,
            allowContinuousDrag: true, continuousSpacingBySize: true);

        /// <summary>Small iron brick.</summary>
        public static readonly PlaceableDefinition Block = Def(
            "Block", "block_small", "iron brick", 6, 3, 0.48f, 0.42f, true, ObjectType.None, 0,
            continuousSpacingBySize: true);

        /// <summary>Wide iron brick variant that disallows overlap.</summary>
        public static readonly PlaceableDefinition BlockHorizontal = Def(
            "BlockHorizontal", "block_wide", "iron brick", 11, 2, 0.58f, 0.52f, false, ObjectType.None, 0,
            continuousSpacingBySize: true);

        /// <summary>Trigger generator placed on walls.</summary>
        public static readonly PlaceableDefinition WallGen = Def(
            "WallGen", "wall_gen", "Trigger Generator", 11, 5, 0.49f, 0.51f, true, ObjectType.Generator, 0);

        /// <summary>Save/craft UI wire variant.</summary>
        public static readonly PlaceableDefinition SaveCraft = Def(
            "SaveCraft", "line", "Electric Wires", 3, 4, 0.56f, 0.55f, true, ObjectType.Wire, 10,
            allowContinuousDrag: true, continuousSpacingBySize: true);

        // -------------------------------------------------------------------------
        // Registration
        // -------------------------------------------------------------------------

        private static readonly PlaceableDefinition[] AllDefinitions =
        {
            Boss3Gen, BossGen, BbConstraint,
            BulletGen, BulletKill,
            CatMachine, Cm, CombatDroneStation, DroneStation, DummyGenerator,
            ElectricBig, ElectricHuge, FakeMan, FearLight, Gg, Gun1, GunMachine,
            LayerTower, Line2, Line3, MachineBaseBaseS10, MoveMachine1, Punch,
            RobotMachine, Runner, SealMachine, Spawn, Spawn2, SpawnPro, Step,
            Stink, Teleport, Windmac, Manbase,
            Gp1, Block, BlockHorizontal, WallGen, SaveCraft
        };

        /// <summary>All 39 archetypes in registration order.</summary>
        public static IReadOnlyList<PlaceableDefinition> All => AllDefinitions;

        /// <summary>Looks up an archetype by its unique catalog key.</summary>
        public static bool TryGet(string id, out PlaceableDefinition definition)
        {
            for (int i = 0; i < AllDefinitions.Length; i++)
            {
                if (AllDefinitions[i].Id != id) continue;
                definition = AllDefinitions[i];
                return true;
            }

            definition = default(PlaceableDefinition);
            return false;
        }

        /// <summary>Looks up an archetype by the original prefab saveId.</summary>
        public static bool TryGetBySaveId(string saveId, out PlaceableDefinition definition)
        {
            for (int i = 0; i < AllDefinitions.Length; i++)
            {
                if (AllDefinitions[i].SaveId != saveId) continue;
                definition = AllDefinitions[i];
                return true;
            }

            definition = default(PlaceableDefinition);
            return false;
        }

        /// <summary>Counts archetypes by power role.</summary>
        public static int CountByRole(ObjectType role)
        {
            int count = 0;
            for (int i = 0; i < AllDefinitions.Length; i++)
            {
                if (AllDefinitions[i].PowerRole == role) count++;
            }
            return count;
        }

        private static PlaceableDefinition Def(
            string id,
            string saveId,
            string displayName,
            int width,
            int height,
            float pivotX,
            float pivotY,
            bool allowOverlap,
            ObjectType role,
            int sortingOrder,
            bool allowContinuousDrag = false,
            bool continuousSpacingBySize = false,
            bool forcePlaceAnywhere = false,
            bool enableLineAssist = false,
            PlaceableItemKind itemKind = PlaceableItemKind.Building,
            int maxStack = 999)
        {
            return new PlaceableDefinition
            {
                Id = id,
                SaveId = saveId,
                DisplayName = string.IsNullOrEmpty(displayName) ? id : displayName,
                Size = new Vector2Int(width, height),
                Pivot01 = new Vector2(pivotX, pivotY),
                AllowOverlap = allowOverlap,
                PowerRole = role,
                ItemKind = itemKind,
                SortingOrderWhenPlaced = sortingOrder,
                AllowContinuousDrag = allowContinuousDrag,
                ContinuousSpacingBySize = continuousSpacingBySize,
                ForcePlaceAnywhere = forcePlaceAnywhere,
                EnableLineAssist = enableLineAssist,
                MaxStack = maxStack
            };
        }
    }
}
