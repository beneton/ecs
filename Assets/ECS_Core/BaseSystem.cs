namespace Beneton.ECS.Core
{
	public abstract class BaseSystem
	{
		public abstract void OnCreate(IArchetypeProvider archetypeProvider);

		public virtual void CleanUp(
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer)
		{
		}

		public abstract void Update(
			float deltaTime,
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer,
			IWorld world);
	}
}