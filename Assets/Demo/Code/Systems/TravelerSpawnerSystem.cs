using Beneton.ECS.Core;
using ECSSample.Components;
using UnityEngine;

namespace ECSSample.Systems
{
	/// <summary>
	/// Handles click on UI buttons to spawn travelers
	/// </summary>
	public class TravelerSpawnerSystem : BaseSystem
	{
		private Archetype _clicked;
		private GameObject _travelerPrefab;

		public TravelerSpawnerSystem(GameObject prefab)
		{
			_travelerPrefab = prefab;
		}

		public override void OnCreate(IArchetypeProvider archetypeProvider)
		{
			_clicked = archetypeProvider.GetOrCreateArchetype(
				new[] { AddTravelerButton.Id, Clicked.Id });
		}

		public override void Update(
			float deltaTime,
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer,
			IWorld world)
		{
			foreach (var entity in componentManager.GetEntities(_clicked))
			{
				commandBuffer.RemoveComponent<Clicked>(entity);

				var button = componentManager.GetComponent<AddTravelerButton>(entity);
				for (var i = 0; i < button.Amount; i++)
				{
					var newTraveler = world.Spawn(_travelerPrefab, null);
					newTraveler.GameObject.transform.position =
						new Vector3(
							Random.Range(-10f, 10f),
							0.5f,
							Random.Range(-10f, 10f));
				}
			}
		}
	}
}