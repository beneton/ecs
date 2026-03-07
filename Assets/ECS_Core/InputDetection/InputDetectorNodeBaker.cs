namespace Beneton.ECS.Core
{
	public abstract class InputDetectorNodeBaker : Baker
	{
		protected IComponentGetter ComponentManager;
		protected ICommandBuffer CommandBuffer;
		protected IWorld World;
		protected Entity Entity;

		internal void Bake(
			Entity entity,
			IComponentManager componentManager,
			ICommandBuffer commandBuffer,
			IWorld world)
		{
			Entity = entity;
			ComponentManager = componentManager;
			CommandBuffer = commandBuffer;
			World = world;
			Bake(entity, componentManager, world);
		}

		protected internal abstract void EcsUpdate(float deltaTime);
		protected internal abstract void CleanUp();
	}
}