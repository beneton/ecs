using Beneton.ECS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ECSSample.Components
{
	public class AddTravelerButtonBaker : InputDetectorNodeBaker
	{
		[SerializeField]
		private Button _button;

		[SerializeField]
		private int _count = 1;

		private void Awake()
		{
			_button.onClick.AddListener(OnButtonClick);
		}

		private void OnButtonClick()
		{
			CommandBuffer.AddComponent(Entity, new Clicked());
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

		protected override void EcsUpdate(float deltaTime)
		{
			
		}

		protected override void CleanUp()
		{
			CommandBuffer.RemoveComponent<Clicked>(Entity);
		}
	}
}