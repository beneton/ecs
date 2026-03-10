using Unity.Entities;
using UnityEngine;

namespace DotsDemo
{
	public class TravelerAuthoring : MonoBehaviour
	{
		public class MovementBaker : Baker<TravelerAuthoring>
		{
			public override void Bake(TravelerAuthoring authoring)
			{
				var entity = GetEntity(TransformUsageFlags.Dynamic);
				AddComponent<Movement>(entity);
				AddComponent<StartMoving>(entity);
				AddComponent<Traveler>(entity);
			}
		}
	}
}