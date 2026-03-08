using System;

namespace Beneton.ECS.Core
{
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