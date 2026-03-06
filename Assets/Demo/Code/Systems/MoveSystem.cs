using Beneton.ECS.Core;
using ECSSample.Components;

namespace ECSSample.Systems
{
	/// <summary>
	/// Moves travelers based on direction and speed
	/// </summary>
	public class MoveSystem : BaseSystem
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