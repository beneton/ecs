### Gotchas

#### 1. Components: Mandatory Partial Structs and Code Generation
All components must be defined as `public partial struct` and implement `IComponent` (or `ISingletonComponent`).
*   **The Gotcha:** If you forget the `partial` keyword or don't implement `IComponent`, your code will fail to compile because `YourComponent.Id` will be missing.
*   **Why:** The framework uses an Editor-time code generator (`ComponentIdGenerator.cs`) to assign a unique integer ID to each component type. These IDs are injected into a partial implementation of your struct in `Assets/_Generated/ComponentIds.g.cs`.
*   **Recognised forms:** `IComponent` (with `using Beneton.ECS.Core;`, or when declared inside the `Beneton.ECS.Core` namespace), `Beneton.ECS.Core.IComponent`, `global::Beneton.ECS.Core.IComponent`, or an alias such as `Core.IComponent` (with `using Core = Beneton.ECS.Core;`). Commented-out code is ignored.
*   **Not supported:** generic components (`struct MyComp<T>`), components nested inside a class, and files declaring more than one namespace.
*   **Tip:** Keep one component per file, in a `Components` folder.

#### 2. Component IDs Are Only Stable for a Given Set of Components
IDs are assigned in alphabetical order of namespace and name.
*   **The Gotcha:** Adding, removing or renaming a component shifts the IDs of every component sorted after it.
*   **Why it matters:** Never save component IDs to disk (save files, assets, network messages). Use them only at runtime.

#### 3. Archetype Filtering vs. Component Access
Systems use `Archetype` objects to query entities.
*   **The Gotcha:** Defining an archetype in `OnCreate` only determines *which* entities the system iterates over. It does *not* automatically provide access to the components.
*   **Behavior:** You must still call `componentManager.GetComponent<T>(entity)` inside your update loop for each component you need.
*   **Note:** An archetype is a query ("must have these components, must not have those"), not a storage group. Archetypes are shared: asking for the same required/excluded set twice returns the same archetype.

#### 4. `GetComponent` vs. `TryGetComponent`
`GetComponent<T>` is deliberately unchecked, for performance.
*   **The Gotcha:** If the entity does not have the component, `GetComponent` does **not** reliably throw. It can silently return another entity's data.
*   **The Rule:** Call `GetComponent` only when you *know* the entity has the component, for example because the archetype you are iterating requires it. When you are not certain, use `TryGetComponent` (or `HasComponent`).

#### 5. Command Buffer Deferral
Every structural change (adding, updating or removing components, and destroying entities) **must** go through the `ICommandBuffer`. Systems only receive read access (`IComponentGetter`), and `IWorld` does not expose `DestroyEntity`.
*   **The Gotcha:** Changes made via `ICommandBuffer` are not applied immediately. They are batched and executed right after the system's `Update` method finishes.
*   **Why:** This prevents "concurrent modification" issues where an entity is added to, or removed from, an archetype while you are still iterating over it.
*   **Tip:** Changes become visible to every system that runs *after* the current one, in the same frame. Only the current system cannot see them: if you add a component via the command buffer, you cannot `GetComponent` it later in the same `Update`.

#### 6. Component Updating Logic
When modifying a component's data, you must explicitly tell the framework to save those changes.
*   **The Gotcha:** Since components are `structs`, calling `var data = componentManager.GetComponent<MyComp>(entity); data.Value = 10;` only modifies a local copy.
*   **The Fix:** You must call `commandBuffer.UpdateComponent(entity, data);` to commit the changes back to the storage.
*   **Note:** `UpdateComponent` is deferred too. If you update the same component twice in one system, the last update wins, and neither is visible until the system finishes.

#### 7. System Execution Order and Registration
Systems are executed in the order they are added to the `World` with `AddSystem`. Systems added with `AddSystemLate` run, in their own order, during `World.LateUpdate`.
*   **The Gotcha:** There is no attribute-based sorting (like `[UpdateAfter]`). If `SystemB` depends on a component added by `SystemA` in the same frame, `SystemA` must be registered first.
*   **Rule:** Register **every** system before calling `World.Start()`. `Start` is when System Nodes found in the scene are registered with their DistributedSystems.
*   **Cleanup Pattern:** Observe `MovementStateCleanupSystem` in the Demo; it's registered early to clear "one-shot" components (like `StartedResting`) before other systems run, ensuring flags don't persist longer than one frame.

#### 8. Entity References Can Go Stale
An `Entity` is an `Id` plus a `Version`. When an entity is destroyed, its `Id` is reused by a later entity with a higher `Version`.
*   **The Gotcha:** If you store an entity inside a component (e.g. a `Target`), that entity may have been destroyed since, and its `Id` may now belong to a different entity.
*   **Behavior:** `world.HasEntity(entity)` and all checked calls (`AddComponent`, `UpdateComponent`, `RemoveComponent`, `TryGetComponent`, `HasComponent`, `TryGetGameObject`) compare the version and reject stale handles. The unchecked `GetComponent` does not: with a stale handle it returns the new entity's data.
*   **The Fix:** Check `world.HasEntity(target)` before using a stored entity.
*   **Note:** `Entity.Null` has Id 0 and Version 0. Live entities start at Version 1, so a default-constructed `Entity` is never valid.

#### 9. Spawning
`World.Spawn(prefab, parent)` follows the same rules as `World.Start`: every GameObject in the instantiated prefab that has a `Baker` or an `ISystemNode` becomes its **own** entity.
*   **The Gotcha:** The result's root `Entity` is `Entity.Null` if the prefab's root GameObject has no Baker or System Node. Child entities are listed in `SpawnResult.Children`.
*   **Tip:** A spawned entity is visible to every system that runs after the one that spawned it, in the same frame. The spawning system's current loop does not visit it.

#### 10. Destroying Entities
*   **The Gotcha:** Destroying an entity does not destroy its GameObject, and destroying a spawned root entity does not destroy its child entities.
*   **The Fix:** The caller is responsible for destroying the GameObject (`Object.Destroy`) and, when needed, the entities in `SpawnResult.Children`.

#### 11. Bakers Write Directly
Bakers receive the full `IComponentManager`, so their changes are applied immediately, without a command buffer.
*   **The Gotcha:** Bakers have no command buffer and `IWorld` has no `DestroyEntity`, so a Baker cannot destroy entities. Bakers are meant only for setting entities up.
