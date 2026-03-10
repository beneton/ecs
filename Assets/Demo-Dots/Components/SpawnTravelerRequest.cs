using Unity.Entities;

namespace DotsDemo
{
	public struct SpawnTravelerRequest : IComponentData
	{
		public int Amount;
	}
}