namespace Beneton.ECS.Core
{
	public abstract class BaseSystem
	{
		public abstract void OnCreate(IArchetypeProvider archetypeProvider);

		public abstract void Update(
			float deltaTime,
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer,
			IWorld world);
	}
}