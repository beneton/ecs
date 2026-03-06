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
		private Archetype _travelers;
		private Archetype _restingTravelers;

		public override void OnCreate(IArchetypeProvider archetypeProvider)
		{
			_newTravelers = archetypeProvider.GetOrCreateArchetype(
				new[] { Traveler.Id, Movement.Id },
				new[] { Resting.Id, DirectionCommitment.Id });

			_travelers = archetypeProvider.GetOrCreateArchetype(
				new[] { Traveler.Id, Movement.Id, DirectionCommitment.Id },
				new[] { Resting.Id });

			_restingTravelers = archetypeProvider.GetOrCreateArchetype(
				new[] { Traveler.Id, Movement.Id, Resting.Id },
				new[] { DirectionCommitment.Id });
		}

		public override void Update(
			float deltaTime,
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer,
			IWorld world)
		{
			// For new travelers, add a Direction Commitment 
			foreach (var entity in componentManager.GetEntities(_newTravelers))
			{
				commandBuffer.AddComponent(
					entity,
					new DirectionCommitment
					{
						Duration = Random.Range(1f, 3f)
					});
			}

			// For travelers that are... hmmm traveling, check if they are still commited to the 
			// direction. If not, let them rest
			foreach (var entity in componentManager.GetEntities(_travelers))
			{
				var commitment = componentManager.GetComponent<DirectionCommitment>(entity);
				commitment.Duration -= deltaTime;
				if (commitment.Duration > 0)
				{
					commandBuffer.UpdateComponent(entity, commitment);
					continue;
				}

				commandBuffer.RemoveComponent<DirectionCommitment>(entity);
				commandBuffer.AddComponent(
					entity,
					new Resting
					{
						Duration = Random.Range(1f, 3f)
					});
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
					new DirectionCommitment
					{
						Duration = Random.Range(1f, 3f)
					});

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