using TMPro;
using Unity.Entities;
using UnityEngine;

namespace DotsDemo
{
	public class EntityCounterAuthoring : MonoBehaviour
	{
		public TextMeshProUGUI TextField;

		public class EntityCounterBaker : Baker<EntityCounterAuthoring>
		{
			public override void Bake(EntityCounterAuthoring authoring)
			{
				var entity = GetEntity(TransformUsageFlags.None);
				AddComponentObject(
					entity,
					new EntityCounterManaged
					{
						TextField = authoring.TextField
					});
			}
		}
	}
}
