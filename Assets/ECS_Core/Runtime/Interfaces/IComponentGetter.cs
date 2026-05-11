using System;

namespace Beneton.ECS.Core
{
	/// <summary>
	/// Defines the interface for querying and retrieving component data and entities.
	/// - Part of the <see cref="ComponentManager"/>'s responsibility split.
	/// - Provides read-only access to components, entity collections, and singleton instances.
	/// </summary>
	public interface IComponentGetter
	{
		ReadOnlySpan<Entity> GetEntities(Archetype archetype);

		T GetComponent<T>(Entity entity) where T : struct, IComponent;

		bool TryGetComponent<T>(Entity entity, out T component) where T : struct, IComponent;

		bool HasComponent<T>(Entity entity) where T : struct, IComponent;

		bool TryGetSingleton<T>(out Entity entity, out T component)
			where T : struct, ISingletonComponent;
	}
}