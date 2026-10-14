using ComponentInterface = Beneton.ECS.Core.IComponent;

namespace Beneton.ECS.Core.Tests
{
	// See TestA for why the id is written by hand
	public struct TestTag : ComponentInterface
	{
		public static int Id => 1003;
		public int TypeId => Id;
	}
}
