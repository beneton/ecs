using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace DotsDemo
{
	public partial struct MovingSystem : ISystem
	{
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var deltaTime = SystemAPI.Time.DeltaTime;

			var ecb = new EntityCommandBuffer(Allocator.Temp);

			foreach (var (transform, movement, moving, entity) in
				SystemAPI.Query<RefRW<LocalTransform>, RefRO<Movement>, RefRW<Moving>>()
					.WithAll<Traveler>()
					.WithEntityAccess())
			{
				transform.ValueRW.Position = transform.ValueRO.Position +
					movement.ValueRO.Direction * movement.ValueRO.Speed * deltaTime;

				moving.ValueRW.Duration -= deltaTime;
				if (moving.ValueRW.Duration <= 0)
				{
					ecb.RemoveComponent<Moving>(entity);
					ecb.AddComponent<StartResting>(entity);
				}
			}

			ecb.Playback(state.EntityManager);
			ecb.Dispose();
		}
	}
}