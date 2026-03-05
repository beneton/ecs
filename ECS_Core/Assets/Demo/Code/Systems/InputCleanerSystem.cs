using Beneton.ECS.Core;
using ECSSample.Components;

namespace ECSSample.Systems
{
	/// <summary>
	///  Cleans up any unhandled click from this frame
	/// </summary>
	public class InputCleanerSystem : BaseSystem
	{
		private Archetype _clicked;

		public override void OnCreate(IArchetypeProvider archetypeProvider)
		{
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
		}
	}
}