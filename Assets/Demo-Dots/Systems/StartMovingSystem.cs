using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.Rendering;

namespace DotsDemo
{
	public partial struct StartMovingSystem : ISystem
	{
		private BatchMaterialID? _movingMaterialID;

		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<Config>();
			state.RequireForUpdate<ConfigManaged>();
		}

		public void OnUpdate(ref SystemState state)
		{
			if (!_movingMaterialID.HasValue)
			{
				var configEntity = SystemAPI.GetSingletonEntity<Config>();
				var configManaged =
					state.EntityManager.GetComponentObject<ConfigManaged>(configEntity);
				var egs = state.World.GetOrCreateSystemManaged<EntitiesGraphicsSystem>();

				_movingMaterialID = egs.RegisterMaterial(configManaged.MovingMaterial);
			}

			var config = SystemAPI.GetSingletonRW<Config>();
			var ecb = new EntityCommandBuffer(Allocator.Temp);

			// Setup new travelers
			foreach (var (startMoving,
					movement,
					transform,
					matMeshInfo,
					entity)
				in
				SystemAPI.Query<
						RefRO<StartMoving>,
						RefRW<Movement>,
						RefRW<LocalTransform>,
						RefRW<MaterialMeshInfo>>()
					.WithEntityAccess())
			{
				matMeshInfo.ValueRW.MaterialID = _movingMaterialID.Value;

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