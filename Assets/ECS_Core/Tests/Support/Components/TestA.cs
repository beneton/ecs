using ComponentInterface = Beneton.ECS.Core.IComponent;

namespace Beneton.ECS.Core.Tests
{
	// Test components declare their id by hand. The generator writes ids into Assets/_Generated,
	// which compiles into Assembly-CSharp, and a partial struct can't span two assemblies.
	// The alias keeps the generator from picking this struct up.
	public struct TestA : ComponentInterface
	{
		public static int Id => 1001;
		public int TypeId => Id;

		public int Value;
	}
}
