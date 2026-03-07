using Beneton.ECS.Core;
using ECSSample.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace ECSSample.Components
{
	public class TravelerBaker : Baker, IPointerClickHandler, IDistributedNode<InputDetectorSystem>
	{
		private bool _wasClicked;

		protected override void Bake(
			Entity entity,
			IComponentManager componentManager,
			IWorld world)
		{
			componentManager.AddComponent(entity, new Traveler());

			var moveDirection =
				new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
			transform.LookAt(transform.position + moveDirection);

			componentManager.AddComponent(
				entity,
				new Movement
				{
					Direction = moveDirection,
					Speed = Random.Range(1f, 3f),
					Transform = transform
				});

			// A cache to avoid calling GetComponents inside systems
			componentManager.AddComponent(
				entity,
				new ECSMeshRenderers
				{
					MeshRenderers = GetComponentsInChildren<MeshRenderer>()
				});
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			_wasClicked = true;
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