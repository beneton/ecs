using System;

namespace Beneton.ECS.Core
{
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