#if UNITY_EDITOR
namespace Beneton.ECS.Core.Editor
{
	public enum TimelineEventType
	{
		AddComponent,
		UpdateComponent,
		RemoveComponent,
		RemoveAllComponents,
		EntityCreated,
		EntityDestroyed
	}

	public struct TimelineEvent
	{
		public int EntityId;
		public string EntityName;
		public string ComponentName;
		public string CallerName;
		public TimelineEventType Type;
		public string FormattedTiming;
	}

	public interface ITimelineHandler
	{
		void SetExecutingBaker(Baker baker);
		void SetExecutingSystem(BaseSystem system);
		void SetExecutingSystem(string systemName);

		void RegisterEntityCreation(Entity entity, World world);
		void RegisterEntityDestruction(Entity entity, World world);
		void RegisterAddComponent(Entity entity, World world, int componentId);
		void RegisterUpdateComponent(Entity entity, World world, int componentId);
		void RegisterRemoveComponent(Entity entity, World world, int componentId);
		void RegisterRemoveAllComponents(Entity entity, World world);
	}
}
#endif