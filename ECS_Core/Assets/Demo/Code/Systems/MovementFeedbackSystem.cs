using Beneton.ECS.Core;
using ECSSample.Components;
using UnityEngine;

namespace ECSSample.Systems
{
	/// <summary>
	/// Change materials based on resting or moving
	/// </summary>
	public class MovementFeedbackSystem : BaseSystem
	{
		private readonly Material _restingMaterial;
		private readonly Material _movingMaterial;

		private Archetype _resting;
		private Archetype _moving;

		public MovementFeedbackSystem(Material resting, Material moving)
		{
			_restingMaterial = resting;
			_movingMaterial = moving;
		}

		public override void OnCreate(IArchetypeProvider archetypeProvider)
		{
			_resting = archetypeProvider.GetOrCreateArchetype(
				new[] { Traveler.Id, Resting.Id, ECSMeshRenderers.Id });

			_moving = archetypeProvider.GetOrCreateArchetype(
				new[] { Traveler.Id, DirectionCommitment.Id, ECSMeshRenderers.Id });
		}

		public override void Update(
			float deltaTime,
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer,
			IWorld world)
		{
			foreach (var entity in componentManager.GetEntities(_resting))
			{
				var ecsMeshRenderers = componentManager.GetComponent<ECSMeshRenderers>(entity);
				foreach (var renderer in ecsMeshRenderers.MeshRenderers)
				{
					renderer.sharedMaterial = _restingMaterial;
				}
			}

			foreach (var entity in componentManager.GetEntities(_moving))
			{
				var ecsMeshRenderers = componentManager.GetComponent<ECSMeshRenderers>(entity);
				foreach (var renderer in ecsMeshRenderers.MeshRenderers)
				{
					renderer.sharedMaterial = _movingMaterial;
				}
			}
		}
	}
}