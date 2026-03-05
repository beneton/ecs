namespace Beneton.ECS.Core
{
	public interface IComponentModifier
	{
		void AddComponent<T>(Entity entity, in T component) where T : struct, IComponent;
		void UpdateComponent<T>(Entity entity, in T component) where T : struct, IComponent;
		void AddOrUpdateComponent<T>(Entity entity, in T component) where T : struct, IComponent;
		void AddMissingComponent<T>(Entity entity, in T component) where T : struct, IComponent;
		void RemoveComponent<T>(Entity entity) where T : struct, IComponent;
	}
}