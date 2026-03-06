using Beneton.ECS.Core;
using UnityEngine;

namespace ECSSample.Components
{
	public class TravelerBaker : Baker
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
	}
}