using Beneton.ECS.Core;
using ECS.Demo.Basic.Components;
using UnityEngine;

namespace ECS.Demo.Basic.Systems
{
	/// <summary>
	/// Manages the movement of travelers: updates positions, increments travel distance, 
	/// and handles transition from <see cref="Moving"/> to <see cref="Resting"/> when duration expires.
	/// </summary>
	public class MovingSystem : BaseSystem
	{
		private Archetype _moving;

		public override void OnCreate(IArchetypeProvider archetypeProvider)
		{
			_moving = archetypeProvider.GetOrCreateArchetype(
				new[] { Traveler.Id, Movement.Id, Moving.Id });
		}

		public override void Update(
			float deltaTime,
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer,
			IWorld world)
		{
			var hasTravelLog = componentManager.TryGetSingleton<TravelLog>(
				out var travelLogEntity,
				out var travelLog);

			foreach (var entity in componentManager.GetEntities(_moving))
			{
				// Update timer and handle transition
				var moving = componentManager.GetComponent<Moving>(entity);
				moving.Duration -= deltaTime;

				if (moving.Duration <= 0)
				{
					commandBuffer.RemoveComponent<Moving>(entity);
					commandBuffer.AddComponent(
						entity,
						new Resting
						{
							Duration = Random.Range(1f, 3f)
						});
					commandBuffer.AddComponent(entity, new StartedResting());
					continue;
				}

				commandBuffer.UpdateComponent(entity, moving);

				// Physical movement
				var movement = componentManager.GetComponent<Movement>(entity);
				var transform = movement.Transform;
				var position = transform.position;

				var travelVector = movement.Direction * (movement.Speed * deltaTime);
				transform.position = position + travelVector;

				if (hasTravelLog)
				{
					travelLog.TotalDistance += travelVector.magnitude;
				}
			}

			if (hasTravelLog)
			{
				commandBuffer.UpdateComponent(travelLogEntity, travelLog);
			}
		}
	}
}