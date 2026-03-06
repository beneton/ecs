using Beneton.ECS.Core;
using TMPro;
using UnityEngine;

namespace ECSSample.Components
{
	public class EntityCounterBaker : Baker
	{
		[SerializeField]
		private TextMeshProUGUI _textField;

		protected override void Bake(
			Entity entity,
			IComponentManager componentManager,
			IWorld world)
		{
			componentManager.AddComponent(
				entity,
				new EntityCounter
				{
					TextField = _textField
				});
		}
	}
}