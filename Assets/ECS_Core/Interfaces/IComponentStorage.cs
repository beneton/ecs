namespace Beneton.ECS.Core
{
	/// <summary>
	/// Represents a type-agnostic interface for component storage.
	/// Used by the <see cref="ComponentManager"/> to manage various component types.
	/// </summary>
	public interface IComponentStorage
	{
		bool HasComponent(Entity entity);
		void Remove(Entity entity);

#if UNITY_EDITOR
		bool TryGetComponentTypeless(Entity entity, out IComponent component);
#endif
	}
}