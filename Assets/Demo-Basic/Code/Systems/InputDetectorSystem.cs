using Beneton.ECS.Core;
using ECS.Demo.Basic.Components;

namespace ECS.Demo.Basic.Systems
{
	/// <summary>
	/// Manages distributed input detection nodes
	/// </summary>
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
			// Removes unhandled clicks from previous frame
			foreach (var entity in componentManager.GetEntities(_clicked))
			{
				commandBuffer.RemoveComponent<Clicked>(entity);
			}

			base.Update(deltaTime, componentManager, commandBuffer, world);
		}
	}
}