using Beneton.ECS.Core;
using ECSSample.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace ECSSample.Components
{
	/// <summary>
	/// Baker responsible for initializing the Add Traveler UI button.
	/// - Bakes the <see cref="AddTravelerButton"/> component with the configured amount.
	/// - Bridges Unity UI <see cref="Button"/> clicks into the ECS by adding a <see cref="Clicked"/> component during the <see cref="InputDetectorSystem"/> update.
	/// </summary>
	public class AddTravelerButtonBaker : Baker, ISystemNode<InputDetectorSystem>
	{
		[SerializeField]
		private Button _button;

		[SerializeField]
		private int _count = 1;

		private bool _wasClicked = false;

		private void Awake()
		{
			_button.onClick.AddListener(OnButtonClick);
		}

		private void OnButtonClick()
		{
			_wasClicked = true;
		}

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

		void ISystemNode.EcsUpdate(
			float deltaTime,
			Entity entity,
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer,
			IWorld world)
		{
			if (!_wasClicked)
			{
				return;
			}

			commandBuffer.AddComponent(entity, new Clicked());
			_wasClicked = false;
		}
	}
}