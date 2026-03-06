using System;

namespace Beneton.ECS.Core
{
	public class ComponentStorage<T> : IComponentStorage where T : struct, IComponent
	{
		// Key is Entity.Id
		private readonly SparseSet<T> _components = new();

		public ReadOnlySpan<int> Keys => _components.Keys;
		public ReadOnlySpan<T> Values => _components.Values;

		public int Length => _components.Length;

		public void Set(Entity entity, in T component)
		{
			_components.Set(entity, component);
		}

		public void Remove(Entity entity)
		{
			_components.Remove(entity);
		}

		public T Get(Entity entity)
		{
			return _components.Get(entity);
		}

		public bool TryGet(Entity entity, out T component)
		{
			component = _components.TryGet(entity, out var exists);
			return exists;
		}

		public bool HasComponent(Entity entity)
		{
			return _components.Has(entity);
		}

#if UNITY_EDITOR
		public bool TryGetComponentTypeless(Entity entity, out IComponent component)
		{
			component = _components.TryGet(entity, out var exists);
			return exists;
		}
#endif
	}
}