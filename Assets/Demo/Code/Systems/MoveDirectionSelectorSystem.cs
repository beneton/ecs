using Beneton.ECS.Core;
using ECSSample.Components;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ECSSample.Systems
{
	/// <summary>
	/// Decides when travelers should change direction and rest
	/// </summary>
	public class MoveDirectionSelectorSystem : BaseSystem
	{
		private Archetype _newTravelers;
		private Archetype _moving;
		private Archetype _restingTravelers;

		private Archetype _startedMoving;
		private Archetype _startedResting;

		public override void OnCreate(IArchetypeProvider archetypeProvider)
		{
			_newTravelers = archetypeProvider.GetOrCreateArchetype(
				new[] { Traveler.Id, Movement.Id },
				new[] { Resting.Id, Moving.Id });

			_moving = archetypeProvider.GetOrCreateArchetype(
				new[] { Traveler.Id, Movement.Id, Moving.Id });

			_restingTravelers = archetypeProvider.GetOrCreateArchetype(
				new[] { Traveler.Id, Movement.Id, Resting.Id });

			_startedMoving = archetypeProvider.GetOrCreateArchetype(
				new[] { StartedMoving.Id });

			_startedResting = archetypeProvider.GetOrCreateArchetype(
				new[] { StartedResting.Id });
		}

		public override void Update(
			float deltaTime,
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer,
			IWorld world)
		{
			// Cleaning up last frame's "started"
			foreach (var entity in componentManager.GetEntities(_startedMoving))
			{
				commandBuffer.RemoveComponent<StartedMoving>(entity);
			}

			// Cleaning up last frame's "started"
			foreach (var entity in componentManager.GetEntities(_startedResting))
			{
				commandBuffer.RemoveComponent<StartedResting>(entity);
			}

			// For new travelers, add a Direction Commitment 
			foreach (var entity in componentManager.GetEntities(_newTravelers))
			{
				commandBuffer.AddComponent(
					entity,
					new Moving
					{
						Duration = Random.Range(1f, 3f)
					});
			}

			// For travelers that are... hmmm traveling, check if they are still commited to the 
			// direction. If not, let them rest
			foreach (var entity in componentManager.GetEntities(_moving))
			{
				var commitment = componentManager.GetComponent<Moving>(entity);
				commitment.Duration -= deltaTime;
				if (commitment.Duration > 0)
				{
					commandBuffer.UpdateComponent(entity, commitment);
					continue;
				}

				commandBuffer.RemoveComponent<Moving>(entity);
				commandBuffer.AddComponent(
					entity,
					new Resting
					{
						Duration = Random.Range(1f, 3f)
					});
				commandBuffer.AddComponent(entity, new StartedResting());
			}

			// For resting travelers, check if they finished their resting and assign a new Direction
			foreach (var entity in componentManager.GetEntities(_restingTravelers))
			{
				var resting = componentManager.GetComponent<Resting>(entity);
				resting.Duration -= deltaTime;

				if (resting.Duration > 0)
				{
					commandBuffer.UpdateComponent(entity, resting);
					continue;
				}

				commandBuffer.RemoveComponent<Resting>(entity);
				commandBuffer.AddComponent(
					entity,
					new Moving
					{
						Duration = Random.Range(1f, 3f)
					});
				commandBuffer.AddComponent(entity, new StartedMoving());

				var movement = componentManager.GetComponent<Movement>(entity);
				movement.Direction = new Vector3(
						Random.Range(-1f, 1f),
						0,
						Random.Range(-1f, 1f))
					.normalized;
				commandBuffer.UpdateComponent(entity, movement);
			}
		}
	}
}