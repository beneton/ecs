using System;

namespace Beneton.ECS.Core
{
	/// <summary>
	/// Represents an abstract system that distributes its <see cref="Update"/> loop among multiple <see cref="ISystemNode"/> instances.
	/// - Enables building systems that are composed of multiple independent logic nodes tied to specific entities.
	/// - Provides a bridge for Unity <see cref="UnityEngine.MonoBehaviour"/>s to function as ECS logic nodes while maintaining secure access to the world.
	/// - A common use case is handling input or other event-driven behaviors where per-entity node logic is preferred over a centralized loop.
	/// </summary>
	/// <typeparam name="T">The type of the concrete system implementing the distributed logic.</typeparam>
	public abstract class DistributedSystem<T> : BaseSystem, IDistributedSystem
		where T : BaseSystem
	{
		protected struct NodeEntityPair
		{
			public Entity Entity;
			public ISystemNode Node;
		}

		public Type SystemType => typeof(T);

		protected readonly SparseSet<NodeEntityPair> NodePairs = new();
		private int _nodeId = 1;

		public void RegisterNode(ISystemNode node, Entity entity)
		{
			NodePairs.Set(
				_nodeId,
				new NodeEntityPair
				{
					Node = node, Entity = entity
				});
			_nodeId++;
		}

		public void UnregisterNode(Entity entity)
		{
			foreach (var nodeId in NodePairs.Keys)
			{
				var pair = NodePairs.Get(nodeId);
				if (pair.Entity == entity)
				{
					NodePairs.Remove(nodeId);
					return;
				}
			}
		}

		public override void OnCreate(IArchetypeProvider archetypeProvider)
		{
		}

		public override void Update(
			float deltaTime,
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer,
			IWorld world)
		{
			foreach (var pair in NodePairs.Values)
			{
				pair.Node.EcsUpdate(deltaTime, pair.Entity, componentManager, commandBuffer, world);
			}
		}
	}
}