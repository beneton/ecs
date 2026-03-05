namespace Beneton.ECS.Core
{
	public static class ComponentTypeIdProvider
	{
		private static int _id = 0;

		public static int Next()
		{
			return ++_id;
		}
	}
}