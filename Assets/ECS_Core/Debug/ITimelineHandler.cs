namespace Beneton.ECS.Core.Editor
{
	public enum TimelineEventType
	{
		AddComponent,
		UpdateComponent,
		RemoveComponent,
		RemoveAllComponents
	}

	public struct TimelineEvent
	{
		public int EntityId;
		public string EntityName;
		public int ComponentId;
		public string ComponentName;
		public string CallerName;
		public TimelineEventType Type;
		public string FormattedTiming;
	}

	public interface ITimelineHandler
	{
		void RegisterAddComponent(Entity entity, string entityName, int componentId, string caller);
		void RegisterUpdateComponent(Entity entity, string entityName, int componentId, string caller);
		void RegisterRemoveComponent(Entity entity, string entityName, int componentId, string caller);
		void RegisterRemoveAllComponent(Entity entity, string entityName, string caller);
	}
}