namespace Beneton.ECS.Core.Editor
{
	public class StubTimelineHandler : ITimelineHandler
	{
		public void SetExecutingBaker(Baker baker)
		{
		}

		public void SetExecutingSystem(BaseSystem system)
		{
		}

		public void SetExecutingSystem(string systemName)
		{
		}

		public void RegisterEntityCreation(Entity entity, World world)
		{
		}

		public void RegisterEntityDestruction(Entity entity, World world)
		{
		}

		public void RegisterAddComponent(Entity entity, World world, int componentId)
		{
		}

		public void RegisterUpdateComponent(Entity entity, World world, int componentId)
		{
		}

		public void RegisterRemoveComponent(Entity entity, World world, int componentId)
		{
		}

		public void RegisterRemoveAllComponents(Entity entity, World world)
		{
		}
	}
}