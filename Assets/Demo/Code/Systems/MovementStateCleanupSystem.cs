using Beneton.ECS.Core;
using ECSSample.Components;

namespace ECSSample.Systems
{
	/// <summary>
	/// Removes transient state tags like <see cref="StartedMoving"/> <see cref="StartedResting"/> at the beginning of the frame.
	/// </summary>
	public class MovementStateCleanupSystem : BaseSystem
	{
		private Archetype _startedMoving;
		private Archetype _startedResting;

		public override void OnCreate(IArchetypeProvider archetypeProvider)
		{
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
			foreach (var entity in componentManager.GetEntities(_startedMoving))
			{
				commandBuffer.RemoveComponent<StartedMoving>(entity);
			}

			foreach (var entity in componentManager.GetEntities(_startedResting))
			{
				commandBuffer.RemoveComponent<StartedResting>(entity);
			}
		}
	}
}