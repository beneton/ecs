using NUnit.Framework;
using UnityEngine;

namespace Beneton.ECS.Core.Tests
{
	public class DistributedSystemTests : EcsTestBase
	{
		[Test]
		public void DestroyEntity_UnregistersEveryNodeOfThatEntity()
		{
			var doubleNode = new GameObject("TwoNodes");
			var first = doubleNode.AddComponent<TestNode>();
			var second = doubleNode.AddComponent<TestNode>();
			var other = new GameObject("Other").AddComponent<TestNode>();
			CreateWorld();
			var system = World.AddSystem<TestDistributedSystem>();
			World.Start();
			Assert.AreEqual(3, system.NodeCount);

			World.DestroyEntity(EntityOf(doubleNode));
			World.Update(Dt);

			Assert.AreEqual(1, system.NodeCount);
			CollectionAssert.IsEmpty(first.UpdatedWith);
			CollectionAssert.IsEmpty(second.UpdatedWith);
			Assert.AreEqual(1, other.UpdatedWith.Count);
		}

		[Test]
		public void NodeWrites_GoThroughTheSystemCommandBuffer()
		{
			var gameObject = new GameObject("Node");
			var node = gameObject.AddComponent<TestNode>();
			node.AddTestB = true;
			CreateWorld();
			var entity = Entity.Null;
			var seenByNextSystem = false;

			World.AddSystem<TestDistributedSystem>();
			AddSystem((componentManager, commandBuffer, world) =>
				seenByNextSystem = componentManager.HasComponent<TestB>(entity));
			World.Start();
			entity = EntityOf(gameObject);

			World.Update(Dt);

			CollectionAssert.AreEqual(new[] { false }, node.HadTestB, "the node doesn't see its own write");
			Assert.IsTrue(seenByNextSystem);
		}
	}
}
