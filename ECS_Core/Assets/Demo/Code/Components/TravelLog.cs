using Beneton.ECS.Core;
using TMPro;

namespace ECSSample.Components
{
	public partial struct TravelLog : ISingletonComponent
	{
		public float TotalDistance;

		public TextMeshProUGUI DistanceTextField;
	}
}