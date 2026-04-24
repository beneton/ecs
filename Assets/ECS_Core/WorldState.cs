namespace Beneton.ECS.Core
{
	public class WorldState
	{
		public string Name;
		internal int Id;
		internal readonly int[] Systems;
		internal readonly int[] LateSystems;

		public WorldState(string name, int[] systems, int[] lateSystems)
		{
			Name = name;
			Systems = systems;
			LateSystems = lateSystems;
		}
	}
}