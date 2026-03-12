Welcome to **Ecs_Core**, a Entity Component System framework made simple, allowing for programers that are used to Object Oriented Programing to experiment with a Data Driven architecture with minimal friction and a gentle learning curve.

### 1. Intention
The primary goal of **Ecs_Core** is to provide a robust **Data-Driven Architecture** for Unity projects. Unlike traditional Object-Oriented Programming (OOP) where data and logic are tightly coupled within MonoBehaviours, **Ecs_Core** separates them into Components (data) and Systems (logic).

**Key Advantages over OOP:**
*   **Reduced Dependencies:** Systems operate on streams of data without needing to know about the specific classes or hierarchies of the objects they process. This decoupling makes the codebase significantly easier to maintain and refactor.
*   **Modular Logic:** Logic is broken down into small, reusable systems that can be easily added, removed, or reordered.
*   **Improved Maintainability:** By isolating state from behavior, you avoid the "spaghetti code" that often arises from complex inheritance trees and inter-object references.

### 2. Why not Unity Entities (DOTS)?
While Unity's DOTS (Data-Oriented Technology Stack) is incredibly powerful for high-end performance optimization, it comes with significant trade-offs:
*   **API Complexity:** DOTS has a steep learning curve and a complex API that can be overwhelming for many projects.
*   **Hidden Magic:** Many simple operations in DOTS involve hidden boilerplate or "magic" that makes debugging and understanding the flow difficult.

**Ecs_Core** fills the middle ground:
*   **Simple API:** It provides a straightforward, easy-to-understand API that feels natural to C# and Unity developers.
*   **Solid Architecture:** It enforces a clean data-driven structure without the overhead of the Job System or Burst Compiler requirements.
*   **Balanced Performance:** While it doesn't match the extreme throughput of DOTS, it offers excellent performance for most game types through efficient data structures like Sparse Sets.

### 3. How it works
The core of **Ecs_Core** is built around several key specialized components:

*   **World:** The main container for the ECS. It manages the lifecycle of entities and orchestrates the execution of systems.
*   **ComponentManager:** Responsible for storing and managing all component data. It handles the mapping between entities and their data.
*   **CommandBuffer:** Allows for deferred modifications to entities (adding/removing components). Systems record changes into the buffer, which are then applied as a batch at the end of the system's update to maintain structural integrity.
*   **Bakers:** Bridge the gap between Unity's authoring environment (GameObjects) and the ECS. Bakers "bake" MonoBehaviour data into ECS Components during initialization.
*   **Archetypes:** Efficiently group entities that share the same set of component types. They are used to quickly query and filter entities for system processing.

### 4. Demo
The repository includes a **Demo** folder containing a small, practical example. It demonstrates how to set up the World, create components, write systems, and use Bakers to convert scene objects into ECS entities.

### 5. Extras
**Ecs_Core** includes several advanced features to further optimize and organize your project:

*   **SparseSet:** Used internally for high-performance mapping of entities to components. It provides O(1) lookup, insertion, and removal while keeping data contiguous in memory for cache-friendly iteration.
*   **DistributedSystem:** A specialized system type that allows you to distribute logic among multiple `ISystemNode` instances. This is particularly useful for bridging Unity MonoBehaviours (like input handlers) into the ECS flow while ensuring that any change made to an Entity or Component is applied at a safe moment.
*   **SingletonComponents:** Components marked with `ISingletonComponent` are guaranteed to have only one instance per World. They are ideal for global state, configuration settings, or unique markers that shouldn't be duplicated across multiple entities.

### 6. Debugging
**Ecs_Core** provides a set of powerful debugging tools to help you visualize and troubleshoot your ECS world:

*   **ArchetypeInspector:** A visual tool that lists all existing archetypes, their component compositions, and the entities within them. It's useful for understanding how entities are grouped and verifying that they have the expected set of components.
<img width="284" height="820" alt="ArchetypeInspector" src="https://github.com/user-attachments/assets/6d44b320-e427-48e1-959e-3e313abbee47" />

*   **EntityInspector:** Provides a detailed view of individual entities. It allows you to inspect all components attached to an entity and their current data values, making it easier to track state changes during runtime.
<img width="295" height="547" alt="EntityInspector" src="https://github.com/user-attachments/assets/30521bd0-380f-4876-b73f-f70be8a2da7e" />

*   **EcsTimeline:** A diagnostic tool that logs and displays a chronological history of ECS events, such as adding, updating, or removing components. It helps in identifying when and where state changes occur, providing better visibility into the system's execution flow.
<img width="915" height="629" alt="EcsTimeline" src="https://github.com/user-attachments/assets/8d502cad-b506-4e1c-964e-0c85ceaca7a5" />

While in the Unity Editor, all entities will have a GameObject representation in the Hierarchy Window. This way it is possible to quickly find a specific Entity of interest and use the EntityInspector to get inspect its Components
These tools can be found in the menu **Window/Ecs_Core**, but this repository also provide **UnityInterfaceLayoutForECS.wlt**, a layout that can be loaded in Unity Editor to set a recommended window layout when working with Ecs_Core 

### Gotchas
A collection of interesting things to keep in mind when using this ECS Framework were collected in [Gotchas.md](Gotchas.md)
