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
		public TimelineEventType Type;
		public int FrameCount;
		public float Realtime;
		public string FormattedMessage;
	}

	public interface ITimelineHandler
	{
		void RegisterAddComponent(Entity entity, string entityName, int componentId);
		void RegisterUpdateComponent(Entity entity, string entityName, int componentId);
		void RegisterRemoveComponent(Entity entity, string entityName, int componentId);
		void RegisterRemoveAllComponent(Entity entity, string entityName);
	}
}