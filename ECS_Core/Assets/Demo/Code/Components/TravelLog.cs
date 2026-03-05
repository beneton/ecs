using Beneton.ECS.Core;

namespace ECSSample.Components
{
	public partial struct TravelLog : IComponent
	{
		public float TotalDistance;
		public int DirectionChangeCount;
	}
}