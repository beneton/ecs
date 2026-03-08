namespace Beneton.ECS.Core
{
	/// <summary>
	/// Provides globally unique, incrementing integer identifiers for component types.
	/// - Role: Serves as the central generator for Component Type IDs, which are used to uniquely identify each component type within the ECS.
	/// - Importance: These IDs are critical for the ECS's performance, as they enable O(1) lookups in <see cref="SparseSet{T}"/>
	/// - Usage: Typically called by auto-generated code to assign a static ID to each component struct.
	/// </summary>
	public static class ComponentTypeIdProvider
	{
		private static int _id = 0;

		public static int Next()
		{
			return ++_id;
		}
	}
}