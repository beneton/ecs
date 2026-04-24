using Beneton.ECS.Core;
using ECSSample.Systems;
using UnityEngine;

namespace ECSSample
{
	/// <summary>
	/// Acts as the main entry point and orchestrator for the ECS lifecycle within the Unity scene.
	/// It is responsible for initializing the <see cref="World"/>, registering and configuring systems, 
	/// and bridging Unity's MonoBehaviour update cycles (<c>Update</c>, <c>LateUpdate</c>) to the ECS framework.
	/// </summary>
	public class WorldRunner : MonoBehaviour
	{
		[Header("Travelers")]
		[SerializeField]
		private GameObject _travelerPrefab;

		[SerializeField]
		private Transform _travelerContainer;

		[Header("Traveler Materials")]
		[SerializeField]
		private Material _restingMaterial;

		[SerializeField]
		private Material _movingMaterial;

		private World _world;

		private void Start()
		{
			_world = new World();

			// Input
			_world.AddSystem<InputDetectorSystem>();

			// Movement
			_world.AddSystem<MovementStateCleanupSystem>();
			_world.AddSystem<TravelerInitializationSystem>();
			_world.AddSystem<RestingSystem>();
			_world.AddSystem<MovingSystem>();
			_world.AddSystem(new MovementFeedbackSystem(_restingMaterial, _movingMaterial));

			// Spawn
			_world.AddSystem(new TravelerSpawnerSystem(_travelerPrefab, _travelerContainer));
			_world.AddSystem<TravelerDespawnerSystem>();

			// UI Update
			_world.AddSystem<TravelLogUpdateSystem>();
			_world.AddSystem<EntityCounterUpdateSystem>();

			_world.Start();
		}

		private void Update()
		{
			_world.Update(Time.deltaTime);
		}

		private void LateUpdate()
		{
			_world.LateUpdate(Time.fixedDeltaTime);
		}
	}
}