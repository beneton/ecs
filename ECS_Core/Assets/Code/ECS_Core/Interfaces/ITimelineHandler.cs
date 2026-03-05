namespace Beneton.ECS.Core
{
	public interface ITimelineHandler
	{
		void RegisterAddComponent(Entity entity, int componentId);
		void RegisterUpdateComponent(Entity entity, int componentId);
		void RegisterRemoveComponent(Entity entity, int componentId);
		void RegisterRemoveAllComponent(Entity entity);
	}
}