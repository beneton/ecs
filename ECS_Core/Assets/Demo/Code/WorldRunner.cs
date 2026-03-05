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
		[Header("Traveler Prefab")]
		[SerializeField]
		private GameObject _travelerPrefab;

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
			_world.AddSystem<MoveDirectionSelectorSystem>();
			_world.AddSystem<MoveSystem>();
			_world.AddSystem(new MovementFeedbackSystem(_restingMaterial, _movingMaterial));

			// Spawn
			_world.AddSystem(new TravelerSpawnerSystem(_travelerPrefab));
			_world.AddSystem<TravelerDespawner>();

			// Input Cleanup
			_world.AddSystem<InputCleanerSystem>();

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