using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace DotsDemo
{
	public partial struct RestingSystem : ISystem
	{
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var deltaTime = SystemAPI.Time.DeltaTime;

			var ecb = new EntityCommandBuffer(Allocator.Temp);

			foreach (var (transform, resting, entity) in
				SystemAPI.Query<RefRW<LocalTransform>, RefRW<Resting>>()
					.WithAll<Traveler>()
					.WithEntityAccess())
			{
				resting.ValueRW.Duration -= deltaTime;
				if (resting.ValueRW.Duration <= 0)
				{
					ecb.RemoveComponent<Resting>(entity);
					ecb.AddComponent<StartMoving>(entity);
				}
			}

			ecb.Playback(state.EntityManager);
			ecb.Dispose();
		}
	}
}