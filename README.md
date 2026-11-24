# Theseus Core - Grid Placement

A dependency-light Unity UPM package containing the grid placement and placeable-object engine extracted from **Theseus' Cycle** (a 2D factory-automation game on Steam). Everything here is engine architecture: no game content, no art, no scenes, no prefabs, no project settings, no localization data, and no third-party runtime dependencies.

The package targets Unity 2021.3 or newer. It uses only `System.*` and `UnityEngine.*`.

## Why this architecture

The original game kept placement, occupancy, power topology, animation and audio in a handful of tightly coupled singletons. This package keeps the algorithms and the design decisions, but removes every game-specific dependency.

- **Pure C# core.** `GridPlacementService`, `GridCoordinateSystem`, `GridOccupancyMap`, `PowerNetworkService`, `InventoryService` and `CraftingService` contain no `MonoBehaviour`, no `Input`, no `Camera` and no UI. They can be unit-tested without a scene.
- **Explicit contracts.** `IPlaceable`, `IPlacementRule`, `IGridBehaviour`, `IPowerGenerator`, `IPowerConsumer`, `IPowerStorage`, `IPlaceableActivator`, `IPlacementInput` and `ICursorWorldResolver` define the seams. Game code plugs in instead of being edited in.
- **Inheritance and composition.** Subclass `PlaceableObject` for a family of objects, or attach any number of `IGridBehaviour` components for per-instance roles. Wire connectivity, pressure triggers and teleport links are implemented as behaviours.
- **Zero allocation per frame.** Scratch buffers are owned by the services and reused. No LINQ, no iterator allocations, no per-frame arrays. Occupancy lookups are O(1). Power rebuilds reuse one BFS queue and one visited set per service.
- **Batched update loops.** `FactoryAnimationManager` registers clients and advances all sprite animations in a single update loop instead of one `Update` per object. `PowerNetworkService` defers reconnection to one BFS rebuild per frame instead of rebuilding on every removal.
- **Throttled audio emitters.** `FactoryAudioManager` pools `AudioSource` instances, caps simultaneous emitters per sound id and applies a cooldown to eliminate the GC/voice spikes that come from one-shot `PlayClipAtPoint` calls.
- **Swappable lifetime.** `IPlaceableActivator` lets a prefab pool replace `Instantiate`/`Destroy` without touching placement code.
- **Code-only data catalogs.** `PlaceableCatalog` describes all 39 concrete archetypes and `CraftingRecipeCatalog` all 32 recipes as plain C# values. No ScriptableObject, prefab or Addressable reference leaks into the package.

## Repository layout

```text
TheseusCycle-Unity-CoreSystems/
├── package.json
├── README.md
├── LICENSE
├── .gitignore
├── Runtime/
│   ├── TheseusCore.Grid.asmdef
│   └── Grid/
│       ├── Contracts/       IPlaceable, IPlaceableActivator, IPlacementInput,
│       │                    IPlacementRule, IGridBehaviour, ICursorWorldResolver,
│       │                    ObjectEnums, PowerContracts
│       ├── Core/            GridCoordinateSystem, GridOccupancyMap, PlacementResult,
│       │                    PlacementRules, GridPlacementService, PowerNetwork,
│       │                    PowerNetworkService, InventoryService, CraftingService,
│       │                    CraftingRecipeCatalog
│       ├── Runtime/         PlaceableObject, PlaceableFactory, DefaultPlaceableActivator,
│       │                    GhostPresenter, LegacyPlacementInput, MouseCursorWorldResolver,
│       │                    GridPlacementController, CraftingRecipe, PowerComponents,
│       │                    FactoryAnimationManager, FactoryAudioManager
│       └── Objects/         PlaceableDefinition, PlaceableCatalog, concrete placeable
│                            subclasses and grid behaviours (wire, pressure, teleport)
└── Samples~/MinimalUsageExamples/
    ├── TheseusCore.Grid.Samples.asmdef
    └── ProgrammaticPlacementExample.cs
```

No `.meta` files, prefabs, scenes, textures, audio clips or project settings are included. Unity generates its own metadata on import.

## Installation

In Unity, open **Window > Package Manager > + > Add package from git URL** and enter:

```text
https://github.com/kenjeii/Theseus-Cycle-grid-electricity-factory-sandbox-system-script-opensource.git
```

Or add this line to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.kenjeii.theseus-core-grid": "https://github.com/kenjeii/Theseus-Cycle-grid-electricity-factory-sandbox-system-script-opensource.git"
  }
}
```

## Grid placement

```csharp
using TheseusCore.Grid;
using UnityEngine;

var coordinates = new GridCoordinateSystem(Vector2.zero, 1f, new RectInt(0, 0, 32, 32));
var service = new GridPlacementService(coordinates);
service.AddRule(new BoundsRule());
service.AddRule(new NoOverlapRule());

Vector2Int desired = coordinates.BottomLeftFromWorld(mouseWorld, new Vector2Int(2, 1), Vector2.zero);
PlacementResult result = service.Resolve(
    desired,
    new Vector2Int(2, 1),
    Vector2.zero,
    allowOverlap: false,
    clampInsideBounds: true,
    useEdgeSnap: true,
    fallbackSearchRadius: 4);

if (result.Success)
{
    // result.BottomLeft, result.WorldPosition and result.Resolution are ready.
    // Commit validates once more before registering the footprint.
    service.Commit(myPlaceable, result);
}
```

`Resolve` tries the desired cell first, then a four-direction edge push past the outermost overlapping neighbours, then a ring search for the nearest free cell. `PlacementResolution` tells you which path succeeded.

### Scene setup

Add these components to the same GameObject:

1. `PlaceableFactory` - fill the id-to-prefab list. Every prefab needs a `PlaceableObject`.
2. `LegacyPlacementInput` - or your own `IPlacementInput` implementation.
3. `MouseCursorWorldResolver` - or your own `ICursorWorldResolver`.
4. `GridPlacementController` - configure bounds, cell size, rules and ghost colours.

For pooling, call `factory.Initialize(myActivator)` where `myActivator` implements `IPlaceableActivator`:

```csharp
public sealed class PoolActivator : IPlaceableActivator
{
    public GameObject Acquire(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        => MyPool.Spawn(prefab, position, rotation, parent);

    public void Release(GameObject instance) => MyPool.Despawn(instance);
}
```

## Placeable object catalog

`PlaceableCatalog` contains all **39** placeable archetypes from the source project. Each `PlaceableDefinition` carries the original footprint, pivot, overlap flag, power role, inventory kind, sorting order and placement flags. No prefab or sprite is shipped.

```csharp
if (PlaceableCatalog.TryGet("CatMachine", out PlaceableDefinition definition))
{
    definition.ApplyTo(myPlaceable);   // calls PlaceableObject.Configure(...)
}

// Look up by the original prefab saveId when importing existing saves:
if (PlaceableCatalog.TryGetBySaveId("catmachine", out PlaceableDefinition bySave))
{
    bySave.ApplyTo(myPlaceable);
}

int generators = PlaceableCatalog.CountByRole(ObjectType.Generator);
```

Summary by power role (full values are in `PlaceableCatalog.cs`):

| Role | Count | Examples |
|---|---|---|
| `None` | 5 | `Block`, `BlockHorizontal`, `FakeMan`, `Boss3Gen`, `Manbase` |
| `Wire` | 4 | `Line2`, `Line3`, `Gp1`, `SaveCraft` |
| `Generator` | 10 | `CatMachine`, `Runner`, `Step`, `WallGen`, `Cm`, `BulletGen` |
| `Storage` | 3 | `Gg` (500), `ElectricBig` (50 000), `ElectricHuge` (10 000 000) |
| `Consumer` | 17 | `Punch`, `Teleport`, `Stink`, `Spawn`, `LayerTower`, `Windmac` |

`ApplyTo` calls `PlaceableObject.Configure(...)`. The pivot is stored as authored and clamped only when converting between world and grid space, matching the original behaviour. A few archetypes enable `ContinuousSpacingBySize` (wires and iron bricks) so drag-placement produces perfectly adjacent tiles rather than stacking in one cell.

Eight representative `PlaceableObject` subclasses (`BlockPlaceable`, `WirePlaceable`, `TeleportPlaceable`, and so on) show how a family can apply a definition from `Reset()`. They are examples, not a requirement - the catalog itself covers all 39 objects.

## Power network

`PowerNetworkService` builds connected components from grid adjacency. Adding a node merges adjacent networks immediately; removing one marks the network dirty and a single BFS rebuild per frame reconnects the remainder.

```csharp
using TheseusCore.Grid;

var power = new PowerNetworkService(gridPlacementService);

// Register devices after a successful placement.
power.AddNode(placeable);

// Once per frame, e.g. in LateUpdate:
power.RebuildDirtyNetworks();

// Devices learn their hubs through IPowerGenerator.SetNetwork / IPowerConsumer.SetNetwork.
// GeneratorComponent.Generate(Time.deltaTime) deposits into the first storage hub.
// ConsumerComponent.TryConsume(amount, hubs) withdraws from any hub with enough charge.
if (placeable.Consumer != null && placeable.Consumer.TryConsume(50f, null))
{
    FirePunch();
}
```

`GeneratorComponent`, `NuclearGeneratorComponent`, `ConsumerComponent`, `PowerStorageComponent` and `DummyGeneratorComponent` are ready-made `MonoBehaviour` implementations of the power contracts. `PowerStorageComponent` caps at a configurable capacity. All device buffers are owned by the network and reused between notifications.

## Inventory and crafting

```csharp
using TheseusCore.Grid;

var inventory = new InventoryService();
inventory.Add("iron", 30);
inventory.Add("line", 20);

var crafting = new CraftingService(inventory);

// Use a shipped recipe definition, or implement ICraftingRecipe yourself.
if (CraftingRecipeCatalog.TryGet("cat_gen", out RecipeDefinition recipe) && crafting.TryConsume(recipe))
{
    // Ingredients consumed: line 2, iron 2, cat 2, glass 2.
    // Spawn PlaceableCatalog.TryGet(recipe.ResultId, ...) as needed.
}

// A ScriptableObject recipe asset is also supported, with no prefab reference required:
// [CreateAssetMenu] public sealed class CraftingRecipe : ScriptableObject, ICraftingRecipe
```

`CraftingRecipeCatalog` contains all **32** recipes from the source project, transcribed from the ScriptableObject assets. Each `RecipeDefinition` holds a result catalog id, result count and a fixed array of `CraftingIngredient` values. `line` produces 50 wires from 5; `block` produces 10 from 15 iron; the remaining 30 recipes are stored with their exact original ingredient lists.

`InventoryService` is an allocation-conscious stack inventory with a stable slot list. It implements `IInventory` (`Count`, `Remove`, `Add`) and can be serialized by any save layer.

## Managers

- **`FactoryAnimationManager`** - a single update loop drives all registered `FactoryAnimClient` sprite-renderer animations. `Register`, `Unregister`, and `FrameCount` are the public surface.
- **`FactoryAudioManager`** - pooled `AudioSource` emitters with per-id cooldowns and a maximum concurrent emitter count. `Play(string id)` records playback and returns whether the sound was allowed to start.

```csharp
FactoryAnimationManager.Instance.Register(myAnimClient);
FactoryAudioManager.Instance.Play("place");
```

Both managers avoid the per-object `Update` and the one-shot audio allocation that caused GC spikes in the original implementation.

## Extending the system

**New placeable type** - subclass `PlaceableObject` and override `OnPlaced` / `OnRemoved`, or leave the base class untouched and attach a component implementing `IGridBehaviour`:

```csharp
public sealed class SpawnerBehaviour : MonoBehaviour, IGridBehaviour
{
    public void OnPlaced(IPlaceable placeable, GridPlacementService service) { /* register */ }
    public void OnRemoved() { /* unregister */ }
}
```

`PlaceableObject.NotifyPlaced(service)` and `NotifyRemoved()` dispatch to all attached behaviours.

**New rule** - implement `IPlacementRule` and register it with `service.AddRule(...)`. Rules are evaluated in insertion order with short-circuiting.

**New input source** - implement `IPlacementInput` (legacy Input, Input System, touch, XR). The controller never reads a device directly.

## Performance notes

- `GridOccupancyMap` lookups are O(1); `Resolve` reuses three `List<Vector2Int>` buffers owned by the service.
- `PlaceableObject.SetPlacement` copies the supplied cell list; the service buffer is reused immediately afterwards.
- `PowerNetworkService` reuses one BFS queue and one visited set. No allocation on a clean rebuild.
- `FactoryAnimationManager` advances every client in one loop; `FactoryAudioManager` pools emitters and throttles by cooldown.
- `GhostPresenter.Show` allocates when the prefab changes. `MoveTo` and `Tint` do not allocate.
- Avoid calling the services from multiple threads; they own mutable scratch buffers.

## License

MIT. Copyright (c) 2026 Lei Yat Ming / kenjeii. See `LICENSE`.
