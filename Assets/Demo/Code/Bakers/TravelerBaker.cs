using Beneton.ECS.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ECSSample.Components
{
	public class TravelerBaker : InputDetectorNodeBaker, IPointerClickHandler
	{
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
			CommandBuffer.AddComponent(Entity, new Clicked());
		}

		protected override void EcsUpdate(float deltaTime)
		{
		}

		protected override void CleanUp()
		{
			// If no one detected the click, remove it
			CommandBuffer.RemoveComponent<Clicked>(Entity);
		}
	}
}