using Beneton.ECS.Core;
using ECSSample.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace ECSSample.Components
{
	/// <summary>
	/// Baker responsible for initializing a Traveler entity.
	/// - Bakes the <see cref="Traveler"/> and <see cref="Movement"/> components with random speed and direction.
	/// - Caches <see cref="MeshRenderer"/> references into the <see cref="EcsMeshRenderers"/> component to avoid expensive Unity API calls during system updates.
	/// - Bridges pointer clicks via <see cref="IPointerClickHandler"/> into the ECS by adding a <see cref="Clicked"/> component during the <see cref="InputDetectorSystem"/> update.
	/// </summary>
	public class TravelerBaker : Baker, IPointerClickHandler, ISystemNode<InputDetectorSystem>
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
				new EcsMeshRenderers
				{
					MeshRenderers = GetComponentsInChildren<MeshRenderer>()
				});
		}

		void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
		{
			_wasClicked = true;
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