using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace DotsDemo
{
	public partial struct StartRestingSystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<Config>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var config = SystemAPI.GetSingletonRW<Config>();
			var ecb = new EntityCommandBuffer(Allocator.Temp);

			// Setup new travelers
			foreach (var (startResting, entity) in
				SystemAPI.Query<RefRO<StartResting>>()
					.WithEntityAccess())
			{
				ecb.AddComponent(
					entity,
					new Resting
					{
						Duration = config.ValueRW.Random.NextFloat(1f, 3f)
					});
				ecb.RemoveComponent<StartResting>(entity);
			}

			ecb.Playback(state.EntityManager);
			ecb.Dispose();
		}
	}
}