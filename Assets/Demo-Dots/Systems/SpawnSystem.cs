using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DotsDemo
{
	public partial struct SpawnSystem : ISystem
	{
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<Config>();
		}

		public void OnUpdate(ref SystemState state)
		{
			var config = SystemAPI.GetSingletonRW<Config>();
			var prefab = config.ValueRO.TravelerPrefab;
			var ecb = new EntityCommandBuffer(Allocator.Temp);

			var travelerTransform = state.EntityManager.GetComponentData<LocalTransform>(prefab);

			foreach (
				var (request, entity)
				in
				SystemAPI.Query<RefRO<SpawnTravelerRequest>>().WithEntityAccess())
			{
				for (var i = 0; i < request.ValueRO.Amount; i++)
				{
					var newTraveler = state.EntityManager.Instantiate(prefab);
					travelerTransform.Position = new float3(
						config.ValueRW.Random.NextFloat(-10f, 10f),
						0.5f,
						config.ValueRW.Random.NextFloat(-10f, 10f)
					);
					state.EntityManager.SetComponentData(newTraveler, travelerTransform);
				}

				ecb.DestroyEntity(entity);
			}

			ecb.Playback(state.EntityManager);
			ecb.Dispose();
		}
	}
}