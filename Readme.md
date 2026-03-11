### Ecs Demo Overview
The `ECSDemo` scene is a demonstration of a custom Entity Component System (ECS) framework. It features a "Traveler" simulation where entities move around, rest. More travelers can be created using buttons in the HUD and clicking on a traveler will remove it from the simulation.

This demo showcases:
- The setup of the ECS architecture
- How to move a Game Object around
- How to apply changes in MonoBehaviours, in this case, MeshRenderers
- How to spawn new Game Objects
- How to destroy Game Objects
- The connection between UI events and the ECS world
- Updating UI based on Component data
- Different ways of using a Component, as pure data, as tags, as MonoBehaviour container, as events
- The concept of Singleton Components
- The concept of Distributed System and SystemNodes

### Entry Point
- **WorldRunner.cs**: This `MonoBehaviour` is the heart of the scene. It initializes the ECS `World`, registers all the systems, and bridges Unity's `Update` and `LateUpdate` loops to the ECS framework.

### Key Components
The scene uses several ECS components to manage the state of entities:
- **Traveler**: A tag component identifying an entity as a traveler.
- **Movement**: Stores movement data such as speed, direction, and a reference to the Unity `Transform`.
- **Moving / Resting**: State components with a `Duration` timer to track how long an entity remains in that state.
- **StartedMoving / StartedResting**: "Event" tags used to trigger logic (like material changes) when entering a state.
- **Clicked**: An event tag added when an entity or UI element is clicked.
- **TravelLog / EntityCounter**: Singleton components used for UI updates (total distance and entity count).
- **EcsMeshRenderers**: Caches references to `MeshRenderer` components to avoid expensive `GetComponent` calls during updates.

### Core Systems
The simulation is managed by a series of systems registered in `WorldRunner.cs`:

#### 1. Input and Interaction
- **InputDetectorSystem**: Uses a "Distributed System" pattern where GameObjects (via `AddTravelerButtonBaker` or `TravelerBaker`) notify the ECS of clicks. It adds a `Clicked` component to the corresponding entity.
- **TravelerSpawnerSystem**: Listens for clicks on the "Add Traveler" UI button. When detected, it spawns new traveler entities from a prefab.
- **TravelerDespawnerSystem**: Listens for clicks on individual traveler GameObjects in the scene. When a traveler is clicked, it destroys both the GameObject and the ECS entity.

#### 2. Movement Logic
- **TravelerInitializationSystem**: Sets up newly spawned travelers by giving them an initial `Moving` state.
- **MovingSystem**: Updates the position of travelers, increments the total distance in the `TravelLog`, and transitions entities to the `Resting` state when their timer expires.
- **RestingSystem**: Counts down the rest time and transitions entities back to `Moving` with a new random direction when the timer expires.
- **MovementStateCleanupSystem**: Runs at the start of the frame to remove the transient `StartedMoving` and `StartedResting` tags.

#### 3. Visuals and UI
- **MovementFeedbackSystem**: Swaps the materials of travelers (e.g., between `movingMaterial` and `restingMaterial`) when they transition between states, using the `StartedMoving/Resting` tags.
- **TravelLogUpdateSystem**: Updates the UI text field with the total accumulated distance.
- **EntityCounterUpdateSystem**: Updates the UI text field with the current number of active entities in the world.
