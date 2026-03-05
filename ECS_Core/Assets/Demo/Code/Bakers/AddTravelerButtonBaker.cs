using Beneton.ECS.Core;
using UnityEngine;

namespace ECSSample.Components
{
	public class AddTravelerButtonBaker : Baker
	{
		[SerializeField]
		private int _count = 1;

		protected override void Bake(
			Entity entity,
			IComponentManager componentManager,
			IWorld world)
		{
			componentManager.AddComponent(
				entity,
				new AddTravelerButton
				{
					Amount = _count
				});
		}
	}
}