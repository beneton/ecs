using Beneton.ECS.Core;
using TMPro;
using UnityEngine;

namespace ECSSample.Components
{
	/// <summary>
	/// Baker responsible for providing a reference to the Entity Counter UI text.
	/// - Bakes the <see cref="EntityCounter"/> component with a reference to a <see cref="TextMeshProUGUI"/> field.
	/// </summary>
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