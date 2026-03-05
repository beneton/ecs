using System;

namespace Beneton.ECS.Core
{
	public interface IComponentGetter
	{
		ReadOnlySpan<Entity> GetEntities(Archetype archetype);
		Entity GetFirstEntity(Archetype archetype);
		T GetComponent<T>(Entity entity) where T : struct, IComponent;
		bool TryGetComponent<T>(Entity entity, out T component) where T : struct, IComponent;
		bool HasComponent<T>(Entity entity) where T : struct, IComponent;
	}
}