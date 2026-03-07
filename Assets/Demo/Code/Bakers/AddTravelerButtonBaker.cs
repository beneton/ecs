using Beneton.ECS.Core;
using ECSSample.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace ECSSample.Components
{
	public class AddTravelerButtonBaker : Baker, IDistributedNode<InputDetectorSystem>
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

		public GameObject GetGameObject()
		{
			return gameObject;
		}

		public void EcsUpdate(
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