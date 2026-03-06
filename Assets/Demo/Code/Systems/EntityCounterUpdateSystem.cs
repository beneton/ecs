using Beneton.ECS.Core;
using ECSSample.Components;

namespace ECSSample.Systems
{
	public class EntityCounterUpdateSystem : BaseSystem
	{
		public override void OnCreate(IArchetypeProvider archetypeProvider)
		{
		}

		public override void Update(
			float deltaTime,
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer,
			IWorld world)
		{
			if (componentManager.TryGetSingleton<EntityCounter>(out _, out var counter))
			{
				counter.TextField.text = world.GetEntities().Length.ToString();	
			}
		}
	}
}