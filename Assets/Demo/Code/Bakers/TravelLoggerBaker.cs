using Beneton.ECS.Core;
using TMPro;
using UnityEngine;

namespace ECSSample.Components
{
	public class TravelLoggerBaker : Baker
	{
		[SerializeField]
		private TextMeshProUGUI _distanceField;

		protected override void Bake(
			Entity entity,
			IComponentManager componentManager,
			IWorld world)
		{
			componentManager.AddComponent(
				entity,
				new TravelLog
				{
					TotalDistance = 0,
					DistanceTextField = _distanceField
				});
		}
	}
}