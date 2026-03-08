using Beneton.ECS.Core;
using ECSSample.Components;

namespace ECSSample.Systems
{
	/// <summary>
	/// Updates the <see cref="EntityCounter"/> singleton UI to display the current total number of entities in the <see cref="World"/>.
	/// </summary>
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