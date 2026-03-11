using Unity.Entities;
using Unity.Burst;

namespace DotsDemo
{
	[UpdateInGroup(typeof(PresentationSystemGroup))]
	public partial struct EntityCounterSystem : ISystem
	{
		private EntityQuery _travelerQuery;

		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			_travelerQuery = state.GetEntityQuery(ComponentType.ReadOnly<Traveler>());
			state.RequireForUpdate<EntityCounterManaged>();
		}

		public void OnUpdate(ref SystemState state)
		{
			var counterManaged = SystemAPI.ManagedAPI.GetSingleton<EntityCounterManaged>();
			if (counterManaged.TextField != null)
			{
				int count = _travelerQuery.CalculateEntityCount();
				counterManaged.TextField.text = count.ToString();
			}
		}
	}
}