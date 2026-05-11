namespace Beneton.ECS.Core
{
	/// <summary>
	/// Serves as the base class for all systems in the ECS.
	/// - Provides <see cref="OnCreate"/> for initialization and Archetype creation
	/// - Provides <see cref="Update"/> for logic execution.
	/// </summary>
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