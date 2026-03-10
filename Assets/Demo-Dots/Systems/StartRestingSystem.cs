using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine.Rendering;

namespace DotsDemo
{
	public partial struct StartRestingSystem : ISystem
	{
		private BatchMaterialID? _restingMaterialID;

		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<Config>();
			state.RequireForUpdate<ConfigManaged>();
		}

		public void OnUpdate(ref SystemState state)
		{
			if (!_restingMaterialID.HasValue)
			{
				var configEntity = SystemAPI.GetSingletonEntity<Config>();
				var configManaged =
					state.EntityManager.GetComponentObject<ConfigManaged>(configEntity);
				var egs = state.World.GetOrCreateSystemManaged<EntitiesGraphicsSystem>();

				_restingMaterialID = egs.RegisterMaterial(configManaged.RestingMaterial);
			}

			var config = SystemAPI.GetSingletonRW<Config>();
			var ecb = new EntityCommandBuffer(Allocator.Temp);

			// Setup new travelers
			foreach (var (startResting, matMeshInfo, entity) in
				SystemAPI.Query<RefRO<StartResting>, RefRW<MaterialMeshInfo>>()
					.WithEntityAccess())
			{
				matMeshInfo.ValueRW.MaterialID = _restingMaterialID.Value;

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