using Beneton.ECS.Core;
using ECSSample.Components;
using UnityEngine;

namespace ECSSample.Systems
{
	/// <summary>
	/// Handles click on travelers to despawn them
	/// </summary>
	public class TravelerDespawnerSystem : BaseSystem
	{
		private Archetype _clickedTravelers;

		public override void OnCreate(IArchetypeProvider archetypeProvider)
		{
			_clickedTravelers = archetypeProvider.GetOrCreateArchetype(
				new[] { Traveler.Id, Clicked.Id });
		}

		public override void Update(
			float deltaTime,
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer,
			IWorld world)
		{
			foreach (var entity in componentManager.GetEntities(_clickedTravelers))
			{
				if (world.TryGetGameObject(entity, out var gameObject))
				{
					Object.Destroy(gameObject);
				}

				world.DestroyEntity(entity);
			}
		}
	}
}