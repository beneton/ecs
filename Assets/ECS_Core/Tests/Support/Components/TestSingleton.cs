using SingletonInterface = Beneton.ECS.Core.ISingletonComponent;

namespace Beneton.ECS.Core.Tests
{
	// See TestA for why the id is written by hand
	public struct TestSingleton : SingletonInterface
	{
		public static int Id => 1004;
		public int TypeId => Id;

		public int Value;
	}
}
