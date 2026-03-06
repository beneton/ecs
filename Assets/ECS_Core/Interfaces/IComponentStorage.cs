namespace Beneton.ECS.Core
{
	public interface IComponentStorage
	{
		bool HasComponent(Entity entity);
		void Remove(Entity entity);

#if UNITY_EDITOR
		bool TryGetComponentTypeless(Entity entity, out IComponent component);
#endif
	}
}