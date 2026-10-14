namespace Beneton.ECS.Core.Tests
{
	public class TestABaker : Baker
	{
		public int Value;

		protected override void Bake(Entity entity, IComponentManager componentManager, IWorld world)
		{
			componentManager.AddComponent(entity, new TestA { Value = Value });
		}
	}
}
