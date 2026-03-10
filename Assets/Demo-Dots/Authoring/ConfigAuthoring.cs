using Unity.Entities;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace DotsDemo
{
	public class ConfigAuthoring : MonoBehaviour
	{
		public Material MovingMaterial;
		public Material RestingMaterial;
		
		public GameObject TravelerPrefab;

		public class ConfigBaker : Baker<ConfigAuthoring>
		{
			public override void Bake(ConfigAuthoring authoring)
			{
				var entity = GetEntity(TransformUsageFlags.None);
				AddComponent(
					entity,
					new Config
					{
						Random = new Random((uint)(UnityEngine.Random.value * 1000)),
						TravelerPrefab = GetEntity(authoring.TravelerPrefab, TransformUsageFlags.Dynamic),
					});

				AddComponentObject(
					entity,
					new ConfigManaged
					{
						MovingMaterial = authoring.MovingMaterial,
						RestingMaterial = authoring.RestingMaterial
					});
			}
		}
	}
}