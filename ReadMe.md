Welcome to **ECS_Core**, an Entity Component System framework made simple, allowing programmers who are used to Object-Oriented Programming to experiment with a Data-Driven architecture with minimal friction and a gentle learning curve.

### 1. Intention
The primary goal of **ECS_Core** is to provide a robust **Data-Driven Architecture** for Unity projects. Unlike traditional Object-Oriented Programming (OOP), where data and logic are tightly coupled within MonoBehaviours, **ECS_Core** separates them into Components (data) and Systems (logic).

**Key Advantages over OOP:**
*   **Reduced Dependencies:** Systems operate on streams of data without needing to know about the specific classes or hierarchies of the objects they process. This decoupling makes the codebase significantly easier to maintain and refactor.
*   **Modular Logic:** Logic is broken down into small, reusable systems that can be easily added, removed, or reordered.
*   **Improved Maintainability:** By isolating state from behavior, you avoid the "spaghetti code" that often arises from complex inheritance trees and inter-object references.

### 2. Why not Unity Entities (DOTS)?
While Unity's DOTS (Data-Oriented Technology Stack) is incredibly powerful for high-end performance optimization, it comes with significant trade-offs:
*   **API Complexity:** DOTS has a steep learning curve and a complex API that can be overwhelming for many projects.
*   **Hidden Magic:** Many simple operations in DOTS involve hidden boilerplate or "magic" that makes debugging and understanding the flow difficult.

**ECS_Core** fills the middle ground:
*   **Simple API:** It provides a straightforward, easy-to-understand API that feels natural to C# and Unity developers.
*   **Solid Architecture:** It enforces a clean data-driven structure without the overhead of the Job System or Burst Compiler requirements.
*   **Balanced Performance:** While it doesn't match the extreme throughput of DOTS, it offers excellent performance for most game types through efficient data structures like Sparse Sets. It is single-threaded, so games with very large entity counts may still be better served by DOTS.

### 3. Quick Start

**Requirements:** Unity 6000.4 or newer.

**Install** the package in your project's `Packages/manifest.json`, either from a local copy:
```json
"beneton.ecs.core": "file:C:/Path/To/ECS/Assets/ECS_Core"
```
or from git:
```json
"beneton.ecs.core": "https://github.com/beneton/ecs.git?path=/Assets/ECS_Core"
```

**Recommended layout:** one type per file, grouped by role.
```
Assets/Code/Gameplay/
  WorldRunner.cs
  Components/   one file per component
  Systems/      one file per system
  Bakers/       one file per baker
```

**A component** (`Components/Velocity.cs`). Components are `public partial struct`s; their IDs are generated for you.
```csharp
using Beneton.ECS.Core;
using UnityEngine;

public partial struct Velocity : IComponent
{
	public Vector3 Value;
}
```
`Components/Position.cs` is identical, with a `Position` struct.

**A baker** (`Bakers/MoverBaker.cs`). Add it to a GameObject in the scene; at start it becomes an entity with these components.
```csharp
using Beneton.ECS.Core;
using UnityEngine;

public class MoverBaker : Baker
{
	[SerializeField]
	private Vector3 _velocity;

	protected internal override void Bake(Entity entity, IComponentManager componentManager, IWorld world)
	{
		componentManager.AddComponent(entity, new Position { Value = transform.position });
		componentManager.AddComponent(entity, new Velocity { Value = _velocity });
	}
}
```

**A system** (`Systems/MoveSystem.cs`).
```csharp
using Beneton.ECS.Core;

public class MoveSystem : BaseSystem
{
	private Archetype _movers;

	public override void OnCreate(IArchetypeProvider archetypeProvider)
	{
		_movers = archetypeProvider.GetOrCreateArchetype(new[] { Position.Id, Velocity.Id });
	}

	public override void Update(
		float deltaTime,
		IComponentGetter componentManager,
		ICommandBuffer commandBuffer,
		IWorld world)
	{
		foreach (var entity in componentManager.GetEntities(_movers))
		{
			// GetComponent is safe here: the archetype requires both components
			var position = componentManager.GetComponent<Position>(entity);
			var velocity = componentManager.GetComponent<Velocity>(entity);

			position.Value += velocity.Value * deltaTime;
			commandBuffer.UpdateComponent(entity, position);
		}
	}
}
```

**The World runner** (`WorldRunner.cs`). Add it to a GameObject in the scene.
```csharp
using Beneton.ECS.Core;
using UnityEngine;

public class WorldRunner : MonoBehaviour
{
	private World _world;

	private void Start()
	{
		_world = new World();

		// Register every system before Start
		_world.AddSystem<MoveSystem>();

		_world.Start();
	}

	private void Update()
	{
		_world.Update(Time.deltaTime);
	}

	private void LateUpdate()
	{
		_world.LateUpdate(Time.deltaTime);
	}
}
```

Before going further, read [Gotchas.md](Gotchas.md): most of them matter from day one.

### 4. How it works
The core of **ECS_Core** is built around several key parts:

*   **Entity:** A lightweight handle made of an `Id` and a `Version`. IDs are reused after an entity is destroyed; the version makes sure an old handle never points at the new entity.
*   **World:** The main container for the ECS. It manages the lifecycle of entities and orchestrates the execution of systems.
*   **ComponentManager:** Responsible for storing and managing all component data. It handles the mapping between entities and their data.
*   **CommandBuffer:** Allows for deferred modifications: adding, updating and removing components, and destroying entities. Each system records changes into its own buffer, which is applied as a batch right after that system's update to maintain structural integrity.
*   **Bakers:** Bridge the gap between Unity's authoring environment (GameObjects) and the ECS. Bakers "bake" MonoBehaviour data into ECS Components. Every GameObject with a Baker or a System Node becomes an entity.
*   **Archetypes:** Queries that select the entities having a given set of components (and, optionally, not having another set). Systems use them to quickly find the entities they process.

**Frame order:**
1.  `World.Start()` runs once: every GameObject in the scene with a Baker or a System Node becomes an entity, Bakers bake their components, and System Nodes register with their DistributedSystem.
2.  `World.Update()` runs each system in the order it was added with `AddSystem`. Right after each system, its command buffer is applied, so the next system sees the changes.
3.  `World.LateUpdate()` does the same for systems added with `AddSystemLate`.

**Spawning:** `World.Spawn(prefab, parent)` instantiates a prefab and applies the same rules as `World.Start()` to every GameObject in it. It returns a `SpawnResult` with the root entity and GameObject, plus a list of child entities.

### 5. Demo
The repository includes a **Demo** folder containing a small, practical example. It demonstrates how to set up the World, create components, write systems, and use Bakers to convert scene objects into ECS entities.

### 6. Extras
**ECS_Core** includes several advanced features to further optimize and organize your project:

*   **SparseSet:** Used internally for high-performance mapping of entities to components. It provides O(1) lookup, insertion, and removal while keeping data contiguous in memory for cache-friendly iteration.
*   **DistributedSystem:** A specialized system type that allows you to distribute logic among multiple `ISystemNode` instances. This is particularly useful for bridging Unity MonoBehaviours (like input handlers) into the ECS flow while ensuring that any change made to an Entity or Component is applied at a safe moment.
*   **SingletonComponents:** Components marked with `ISingletonComponent` are guaranteed to have only one instance per World. They are ideal for global state, configuration settings, or unique markers that shouldn't be duplicated across multiple entities.

### 7. Debugging
**ECS_Core** provides a set of powerful debugging tools to help you visualize and troubleshoot your ECS world:

*   **ArchetypeInspector:** A visual tool that lists all existing archetypes, their component compositions, and the entities within them. It's useful for understanding how entities are grouped and verifying that they have the expected set of components.

<img width="284" height="820" alt="ArchetypeInspector" src="https://github.com/user-attachments/assets/6d44b320-e427-48e1-959e-3e313abbee47" />

*   **EntityInspector:** Provides a detailed view of individual entities. It allows you to inspect all components attached to an entity and their current data values, making it easier to track state changes during runtime.

<img width="295" height="547" alt="EntityInspector" src="https://github.com/user-attachments/assets/30521bd0-380f-4876-b73f-f70be8a2da7e" />

*   **EcsTimeline:** A diagnostic tool that logs and displays a chronological history of ECS events, such as adding, updating, or removing components. It helps in identifying when and where state changes occur, providing better visibility into the system's execution flow. The entity filter accepts an `Id` (`5`, every entity that used that ID) or an `Id:Version` pair (`5:2`, one exact entity).

<img width="915" height="629" alt="EcsTimeline" src="https://github.com/user-attachments/assets/8d502cad-b506-4e1c-964e-0c85ceaca7a5" />

While in the Unity Editor, every entity has a GameObject representation in the Hierarchy Window, named with its `[Id:Version]`. This makes it quick to find a specific Entity of interest and inspect its Components with the EntityInspector.
These tools can be found in the menu **Window/Ecs_Core**. This repository also provides **UnityInterfaceLayoutForECS.wlt**, a layout that can be loaded in the Unity Editor to set a recommended window layout when working with ECS_Core.

### Gotchas
A collection of important things to keep in mind when using this ECS framework is collected in [Gotchas.md](Gotchas.md).
