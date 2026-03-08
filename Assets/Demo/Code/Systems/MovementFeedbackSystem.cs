using Beneton.ECS.Core;
using ECSSample.Components;
using UnityEngine;

namespace ECSSample.Systems
{
	/// <summary>
	/// Updates traveler visuals, such as swapping materials between moving and resting states, using cached <see cref="EcsMeshRenderers"/>.
	/// </summary>
	public class MovementFeedbackSystem : BaseSystem
	{
		private readonly Material _restingMaterial;
		private readonly Material _movingMaterial;

		private Archetype _enteringRest;
		private Archetype _enteringMove;

		public MovementFeedbackSystem(Material resting, Material moving)
		{
			_restingMaterial = resting;
			_movingMaterial = moving;
		}

		public override void OnCreate(IArchetypeProvider archetypeProvider)
		{
			_enteringRest = archetypeProvider.GetOrCreateArchetype(
				new[] { StartedResting.Id, EcsMeshRenderers.Id });

			_enteringMove = archetypeProvider.GetOrCreateArchetype(
				new[] { StartedMoving.Id, EcsMeshRenderers.Id });
		}

		public override void Update(
			float deltaTime,
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer,
			IWorld world)
		{
			foreach (var entity in componentManager.GetEntities(_enteringRest))
			{
				var ecsMeshRenderers = componentManager.GetComponent<EcsMeshRenderers>(entity);
				foreach (var renderer in ecsMeshRenderers.MeshRenderers)
				{
					renderer.sharedMaterial = _restingMaterial;
				}
			}

			foreach (var entity in componentManager.GetEntities(_enteringMove))
			{
				var ecsMeshRenderers = componentManager.GetComponent<EcsMeshRenderers>(entity);
				foreach (var renderer in ecsMeshRenderers.MeshRenderers)
				{
					renderer.sharedMaterial = _movingMaterial;
				}
			}
		}
	}
}