using Beneton.ECS.Core;
using ECSSample.Systems;
using UnityEngine;

namespace ECSSample
{
	/// <summary>
	/// Responsible for creating and updating the ECS World 
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

			// Movement
			_world.AddSystem<MoveDirectionSelectorSystem>();
			_world.AddSystem<MoveSystem>();
			_world.AddSystem(new MovementFeedbackSystem(_restingMaterial, _movingMaterial));

			// Spawn
			_world.AddSystem(new TravelerSpawnerSystem(_travelerPrefab, _travelerContainer));
			_world.AddSystem<TravelerDespawnerSystem>();

			// UI Update
			_world.AddSystem<TravelLogUpdateSystem>();
			_world.AddSystem<EntityCounterUpdateSystem>();

			var allBakers = FindObjectsByType<Baker>(
				FindObjectsInactive.Exclude,
				FindObjectsSortMode.None);

			foreach (var baker in allBakers)
			{
				_world.Bake(baker);
			}

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