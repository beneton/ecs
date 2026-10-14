namespace Beneton.ECS.Core.Tests
{
	public class TestBBaker : Baker
	{
		public int Value;

		protected override void Bake(Entity entity, IComponentManager componentManager, IWorld world)
		{
			componentManager.AddComponent(entity, new TestB { Value = Value });
		}
	}
}
