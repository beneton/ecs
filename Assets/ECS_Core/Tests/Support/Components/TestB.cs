using ComponentInterface = Beneton.ECS.Core.IComponent;

namespace Beneton.ECS.Core.Tests
{
	// See TestA for why the id is written by hand
	public struct TestB : ComponentInterface
	{
		public static int Id => 1002;
		public int TypeId => Id;

		public int Value;
	}
}
