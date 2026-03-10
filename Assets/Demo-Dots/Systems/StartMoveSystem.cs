using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DotsDemo
{
	public partial struct StartMoveSystem : ISystem
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
			foreach (var (startMoving, movement, transform, entity) in
				SystemAPI.Query<RefRO<StartMoving>, RefRW<Movement>, RefRW<LocalTransform>>()
					.WithEntityAccess())
			{
				var direction = math.normalize(
					new float3(
						config.ValueRW.Random.NextFloat(-1f, 1f),
						0,
						config.ValueRW.Random.NextFloat(-1f, 1f)));


				movement.ValueRW.Speed = config.ValueRW.Random.NextFloat(1f, 3f);
				movement.ValueRW.Direction = direction;

				// Calculate LookAt rotation
				transform.ValueRW.Rotation = quaternion.LookRotation(
					movement.ValueRW.Direction,
					math.up());

				ecb.AddComponent(
					entity,
					new Moving
					{
						Duration = config.ValueRW.Random.NextFloat(1f, 3f)
					});
				ecb.RemoveComponent<StartMoving>(entity);
			}

			ecb.Playback(state.EntityManager);
			ecb.Dispose();
		}
	}
}