### Gotchas

#### 1. Mandatory Partial Structs and Code Generation
All components must be defined as `public partial struct` and implement `IComponent`.
*   **The Gotcha:** If you forget the `partial` keyword or don't implement `IComponent`, the `ComponentIdGenerator` will not find your struct.
*   **Why:** The framework uses an Editor-time code generator (`ComponentIdGenerator.cs`) to automatically assign unique integer IDs to each component type. These IDs are injected into a partial implementation of your struct in `Assets/_Generated/ComponentIds.g.cs`. Without this, your code will fail to compile because `YourComponent.Id` will be missing.

#### 2. Archetype Filtering vs. Component Access
Systems use `Archetype` objects to query entities.
*   **The Gotcha:** Defining an archetype in `OnCreate` only determines *which* entities the system iterates over. It does *not* automatically provide access to the components.
*   **Behavior:** You must still manually call `componentManager.GetComponent<T>(entity)` inside your update loop. If you attempt to get a component that isn't part of the archetype (or isn't on that specific entity) an exception will be thrown.

#### 3. Command Buffer Deferral
Structural changes (adding/removing components or destroying entities) should generally be done via the `ICommandBuffer`.
*   **The Gotcha:** Changes made via `ICommandBuffer` are not applied immediately. They are batched and executed after the system's `Update` method finishes.
*   **Why:** This prevents "concurrent modification" issues where an entity's archetype changes while you are still iterating through a list of entities belonging to its old archetype.
*   **Tip:** If you add a component via the command buffer, you cannot call `GetComponent` for that same component later in the *same* system update; it won't exist until the next frame (or the end of the current system's execution).

#### 4. Component Updating Logic
When modifying a component's data, you must explicitly tell the framework to save those changes.
*   **The Gotcha:** Since components are `structs`, calling `var data = componentManager.GetComponent<MyComp>(entity); data.Value = 10;` only modifies a local copy.
*   **The Fix:** You must call `commandBuffer.UpdateComponent(entity, data);` to commit the changes back to the storage.

#### 5. System Execution Order
Systems are executed in the order they are added to the `World` in `WorldRunner.cs`.
*   **The Gotcha:** There is no attribute-based sorting (like `[UpdateAfter]`). If `SystemB` depends on a component added by `SystemA` in the same frame, `SystemA` must be registered first.
*   **Cleanup Pattern:** Observe `MovementStateCleanupSystem` in the Demo; it's registered early to clear "one-shot" components (like `StartedResting`) before other systems run, ensuring flags don't persist longer than one frame.
