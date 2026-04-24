using Beneton.ECS.Core;
using TMPro;

namespace ECS.Demo.Basic.Components
{
	/// <summary>
	/// A singleton component that tracks the total distance traveled by all entities and provides a reference to the UI for display.
	/// </summary>
	public partial struct TravelLog : ISingletonComponent
	{
		public float TotalDistance;

		public TextMeshProUGUI DistanceTextField;
	}
}