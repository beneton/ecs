namespace Beneton.ECS.Core
{
	/// <summary>
	/// Defines the interface for modifying component data and entity composition.
	/// - Part of the <see cref="ComponentManager"/>'s responsibility split.
	/// - Provides methods for adding, updating, and removing components from entities.
	/// </summary>
	public interface IComponentModifier
	{
		void AddComponent<T>(Entity entity, in T component) where T : struct, IComponent;
		void UpdateComponent<T>(Entity entity, in T component) where T : struct, IComponent;
		void AddOrUpdateComponent<T>(Entity entity, in T component) where T : struct, IComponent;
		void AddMissingComponent<T>(Entity entity, in T component) where T : struct, IComponent;
		void RemoveComponent<T>(Entity entity) where T : struct, IComponent;
	}
}