using Beneton.ECS.Core;
using TMPro;
using UnityEngine;

namespace ECSSample.Components
{
	/// <summary>
	/// Baker responsible for providing a reference to the Travel Log distance UI text.
	/// - Bakes the <see cref="TravelLog"/> component with a reference to a <see cref="TextMeshProUGUI"/> field.
	/// </summary>
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