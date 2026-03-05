namespace Beneton.ECS.Core
{
	public interface ICommandBuffer
	{
		void AddComponent<T>(Entity entity, T component) where T : struct, IComponent;
		void UpdateComponent<T>(Entity entity, T component) where T : struct, IComponent;
		void AddOrUpdateComponent<T>(Entity entity, T component) where T : struct, IComponent;
		void AddMissingComponent<T>(Entity entity, T component) where T : struct, IComponent;
		void RemoveComponent<T>(Entity entity) where T : struct, IComponent;
		void Execute(ComponentManager componentManager);
	}
}