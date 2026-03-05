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

			componentManager.AddComponent(
				entity,
				new Movement
				{
					Direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f))
						.normalized,
					Speed = Random.Range(1f, 3f)
				});

			componentManager.AddComponent(
				entity,
				new TravelLog
				{
					DirectionChangeCount = 0,
					TotalDistance = 0
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