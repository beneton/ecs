using Beneton.ECS.Core;
using ECSSample.Components;

namespace ECSSample.Systems
{
	/// <summary>
	/// Updates the <see cref="TravelLog"/> singleton UI to display the accumulated total distance traveled by all entities.
	/// </summary>
	public class TravelLogUpdateSystem : BaseSystem
	{
		public override void OnCreate(IArchetypeProvider archetypeProvider)
		{
		}

		public override void Update(
			float deltaTime,
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer,
			IWorld world)
		{
			var hasTravelLog = componentManager.TryGetSingleton<TravelLog>(
				out _,
				out var travelLog);

			if (hasTravelLog)
			{
				travelLog.DistanceTextField.text = travelLog.TotalDistance.ToString("F0");
			}
		}
	}
}