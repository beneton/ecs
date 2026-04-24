using Beneton.ECS.Core;
using ECS.Demo.Basic.Components;
using UnityEngine;

namespace ECS.Demo.Basic.Systems
{
	/// <summary>
	/// Handles the initial state setup for newly created travelers.
	/// </summary>
	public class TravelerInitializationSystem : BaseSystem
	{
		private Archetype _newTravelers;

		public override void OnCreate(IArchetypeProvider archetypeProvider)
		{
			_newTravelers = archetypeProvider.GetOrCreateArchetype(
				new[] { Traveler.Id, Movement.Id },
				new[] { Resting.Id, Moving.Id });
		}

		public override void Update(
			float deltaTime,
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer,
			IWorld world)
		{
			foreach (var entity in componentManager.GetEntities(_newTravelers))
			{
				commandBuffer.AddComponent(
					entity,
					new Moving
					{
						Duration = Random.Range(1f, 3f)
					});
			}
		}
	}
}