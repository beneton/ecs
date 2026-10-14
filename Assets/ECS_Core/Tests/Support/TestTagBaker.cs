namespace Beneton.ECS.Core.Tests
{
	public class TestTagBaker : Baker
	{
		protected override void Bake(Entity entity, IComponentManager componentManager, IWorld world)
		{
			componentManager.AddComponent(entity, new TestTag());
		}
	}
}
