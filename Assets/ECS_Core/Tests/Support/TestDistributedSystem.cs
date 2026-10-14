namespace Beneton.ECS.Core.Tests
{
	public class TestDistributedSystem : DistributedSystem<TestDistributedSystem>
	{
		public int NodeCount => NodePairs.Length;
	}
}
