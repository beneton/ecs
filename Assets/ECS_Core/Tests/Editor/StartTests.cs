using NUnit.Framework;
using UnityEngine;

namespace Beneton.ECS.Core.Tests
{
	public class StartTests : EcsTestBase
	{
		[Test]
		public void Start_EachGameObjectWithBakersBecomesItsOwnEntity()
		{
			var parent = CreateBaked("Parent", a: 5);
			var child = CreateBaked("Child", a: 7, b: 8, parent: parent.transform);
			var plain = new GameObject("Plain");
			plain.transform.SetParent(parent.transform);
			CreateWorld();
			var parentEntity = Entity.Null;
			var childEntity = Entity.Null;
			int parentA = 0, childA = 0, childB = 0;
			var parentHasB = true;

			AddSystem((componentManager, commandBuffer, world) =>
			{
				parentA = componentManager.GetComponent<TestA>(parentEntity).Value;
				parentHasB = componentManager.HasComponent<TestB>(parentEntity);
				childA = componentManager.GetComponent<TestA>(childEntity).Value;
				childB = componentManager.GetComponent<TestB>(childEntity).Value;
			});
			World.Start();
			parentEntity = EntityOf(parent);
			childEntity = EntityOf(child);

			World.Update(Dt);

			Assert.AreNotEqual(parentEntity, childEntity);
			Assert.IsFalse(World.TryGetEntity(plain, out _), "a GameObject without Bakers or nodes is not an entity");
			Assert.AreEqual(5, parentA);
			Assert.IsFalse(parentHasB);
			Assert.AreEqual(7, childA, "both Bakers bake onto the same entity");
			Assert.AreEqual(8, childB, "both Bakers bake onto the same entity");
		}

		[Test]
		public void Start_GameObjectWithOnlyANode_BecomesAnEntityAndItsNodeRuns()
		{
			var gameObject = new GameObject("Node");
			var node = gameObject.AddComponent<TestNode>();
			CreateWorld();
			World.AddSystem<TestDistributedSystem>();
			World.Start();

			var entity = EntityOf(gameObject);
			World.Update(Dt);

			CollectionAssert.AreEqual(new[] { entity }, node.UpdatedWith);
		}

		[TestCase(false)]
		[TestCase(true)]
		public void Start_InactiveGameObjects_FollowTheWorldPolicy(bool considerInactive)
		{
			var gameObject = CreateBaked("Inactive", a: 1);
			gameObject.SetActive(false);
			CreateWorld(considerInactive);

			World.Start();

			Assert.AreEqual(considerInactive, World.TryGetEntity(gameObject, out _));
		}

		[Test]
		public void Start_RegistersNodesWithLateDistributedSystems()
		{
			var gameObject = new GameObject("Node");
			var node = gameObject.AddComponent<TestNode>();
			CreateWorld();
			World.AddSystemLate<TestDistributedSystem>();
			World.Start();

			World.Update(Dt);
			CollectionAssert.IsEmpty(node.UpdatedWith, "late systems don't run in Update");

			World.LateUpdate(Dt);
			CollectionAssert.AreEqual(new[] { EntityOf(gameObject) }, node.UpdatedWith);
		}
	}
}
