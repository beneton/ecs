using Beneton.ECS.Core;
using ECS.Demo.Basic.Components;
using UnityEngine;

namespace ECS.Demo.Basic.Systems
{
	/// <summary>
	/// Manages entities in the Resting state
	/// and handles transition from <see cref="Resting"/> to <see cref="Moving"/> when duration expires.
	/// </summary>
	public class RestingSystem : BaseSystem
	{
		private Archetype _restingTravelers;

		public override void OnCreate(IArchetypeProvider archetypeProvider)
		{
			_restingTravelers = archetypeProvider.GetOrCreateArchetype(
				new[] { Traveler.Id, Movement.Id, Resting.Id });
		}

		public override void Update(
			float deltaTime,
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer,
			IWorld world)
		{
			foreach (var entity in componentManager.GetEntities(_restingTravelers))
			{
				var resting = componentManager.GetComponent<Resting>(entity);
				resting.Duration -= deltaTime;

				if (resting.Duration > 0)
				{
					commandBuffer.UpdateComponent(entity, resting);
					continue;
				}

				// Transition to Moving
				commandBuffer.RemoveComponent<Resting>(entity);
				commandBuffer.AddComponent(
					entity,
					new Moving
					{
						Duration = Random.Range(1f, 3f)
					});
				commandBuffer.AddComponent(entity, new StartedMoving());

				// Select New Direction
				var dir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
				var movement = componentManager.GetComponent<Movement>(entity);
				movement.Direction = dir;
				movement.Transform.LookAt(movement.Transform.position + movement.Direction);

				commandBuffer.UpdateComponent(entity, movement);
			}
		}
	}
}