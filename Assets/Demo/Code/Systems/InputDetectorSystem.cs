using Beneton.ECS.Core;
using ECSSample.Components;

namespace ECSSample.Systems
{
	public class InputDetectorSystem : DistributedSystem<InputDetectorSystem>
	{
		private Archetype _clicked;

		public override void OnCreate(IArchetypeProvider archetypeProvider)
		{
			base.OnCreate(archetypeProvider);

			_clicked = archetypeProvider.GetOrCreateArchetype(
				new[] { Clicked.Id });
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
			}

			base.Update(deltaTime, componentManager, commandBuffer, world);
		}
	}
}