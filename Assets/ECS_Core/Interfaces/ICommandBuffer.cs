namespace Beneton.ECS.Core
{
	/// <summary>
	/// Defines the interface for a deferred execution buffer for entity component modifications.
	/// - Records component additions, updates, and removals to be executed as a batch.
	/// - Minimizes structural overhead by deferring archetype updates until the entire buffer is processed.
	/// - Typically used within systems to perform thread-safe or delayed entity modifications.
	/// </summary>
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