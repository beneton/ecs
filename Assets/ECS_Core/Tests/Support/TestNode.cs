using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beneton.ECS.Core.Tests
{
	/// <summary>
	/// Records every EcsUpdate call. Optionally adds <see cref="TestB"/> to its entity through the command buffer.
	/// </summary>
	public class TestNode : MonoBehaviour, ISystemNode<TestDistributedSystem>
	{
		public bool AddTestB;

		[NonSerialized]
		public readonly List<Entity> UpdatedWith = new();

		// Whether the entity already had TestB when the node ran
		[NonSerialized]
		public readonly List<bool> HadTestB = new();

		public void EcsUpdate(
			float deltaTime,
			Entity entity,
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer,
			IWorld world)
		{
			UpdatedWith.Add(entity);
			HadTestB.Add(componentManager.HasComponent<TestB>(entity));

			if (AddTestB)
			{
				commandBuffer.AddMissingComponent(entity, new TestB { Value = 1 });
			}
		}
	}
}
