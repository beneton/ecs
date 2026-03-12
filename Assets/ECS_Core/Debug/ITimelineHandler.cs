namespace Beneton.ECS.Core.Editor
{
	public enum TimelineEventType
	{
		AddComponent,
		UpdateComponent,
		RemoveComponent,
		RemoveAllComponents,
		EntityCreated,
		EntityDestroyed,
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
		void RegisterEntityCreation(Entity entity, string entityName, string caller);
		void RegisterEntityDestruction(Entity entity, string entityName, string caller);
		void RegisterAddComponent(Entity entity, string entityName, int componentId, string caller);

		void RegisterUpdateComponent(
			Entity entity,
			string entityName,
			int componentId,
			string caller);

		void RegisterRemoveComponent(
			Entity entity,
			string entityName,
			int componentId,
			string caller);

		void RegisterRemoveAllComponents(Entity entity, string entityName, string caller);
	}
}