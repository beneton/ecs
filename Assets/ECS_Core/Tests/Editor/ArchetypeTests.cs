using System;
using NUnit.Framework;

namespace Beneton.ECS.Core.Tests
{
	public class ArchetypeTests : EcsTestBase
	{
		[Test]
		public void RequiredAndExcludeFilters()
		{
			var onlyA = CreateBaked("A", a: 1);
			var ab = CreateBaked("AB", a: 1, b: 1);
			var abTag = CreateBaked("ABTag", a: 1, b: 1, tag: true);
			CreateBaked("B", b: 1);
			CreateWorld();

			Archetype withA = null, withAB = null, withANoTag = null;
			Entity[] inA = null, inAB = null, inANoTag = null;

			AddSystem(
				(componentManager, commandBuffer, world) =>
				{
					inA = componentManager.GetEntities(withA).ToArray();
					inAB = componentManager.GetEntities(withAB).ToArray();
					inANoTag = componentManager.GetEntities(withANoTag).ToArray();
				},
				archetypes =>
				{
					withA = archetypes.GetOrCreateArchetype(new[] { TestA.Id });
					withAB = archetypes.GetOrCreateArchetype(new[] { TestA.Id, TestB.Id });
					withANoTag = archetypes.GetOrCreateArchetype(new[] { TestA.Id }, new[] { TestTag.Id });
				});
			World.Start();

			World.Update(Dt);

			CollectionAssert.AreEquivalent(new[] { EntityOf(onlyA), EntityOf(ab), EntityOf(abTag) }, inA);
			CollectionAssert.AreEquivalent(new[] { EntityOf(ab), EntityOf(abTag) }, inAB);
			CollectionAssert.AreEquivalent(new[] { EntityOf(onlyA), EntityOf(ab) }, inANoTag);
		}

		[Test]
		public void Membership_FollowsAddAndRemove()
		{
			var gameObject = CreateBaked("E", a: 1);
			CreateWorld();
			var entity = Entity.Null;
			var frame = 0;
			Archetype withAB = null, withANoB = null;
			Entity[] inAB = null, inANoB = null;

			AddSystem(
				(componentManager, commandBuffer, world) =>
				{
					if (frame == 1)
					{
						commandBuffer.AddComponent(entity, new TestB());
					}
					else if (frame == 2)
					{
						commandBuffer.RemoveComponent<TestA>(entity);
					}
				},
				archetypes =>
				{
					withAB = archetypes.GetOrCreateArchetype(new[] { TestA.Id, TestB.Id });
					withANoB = archetypes.GetOrCreateArchetype(new[] { TestA.Id }, new[] { TestB.Id });
				});
			AddSystem((componentManager, commandBuffer, world) =>
			{
				inAB = componentManager.GetEntities(withAB).ToArray();
				inANoB = componentManager.GetEntities(withANoB).ToArray();
			});
			World.Start();
			entity = EntityOf(gameObject);

			World.Update(Dt);
			CollectionAssert.AreEqual(Array.Empty<Entity>(), inAB, "baked with A only");
			CollectionAssert.AreEqual(new[] { entity }, inANoB, "baked with A only");

			frame = 1;
			World.Update(Dt);
			CollectionAssert.AreEqual(new[] { entity }, inAB, "after adding B");
			CollectionAssert.AreEqual(Array.Empty<Entity>(), inANoB, "after adding B");

			frame = 2;
			World.Update(Dt);
			CollectionAssert.AreEqual(Array.Empty<Entity>(), inAB, "after removing A");
			CollectionAssert.AreEqual(Array.Empty<Entity>(), inANoB, "after removing A");
		}

		[Test]
		public void GetOrCreateArchetype_SharesArchetypesRegardlessOfOrder()
		{
			CreateWorld();
			Archetype ab = null, ba = null, aOnly = null, aNoB = null;

			AddSystem(null, archetypes =>
			{
				ab = archetypes.GetOrCreateArchetype(new[] { TestA.Id, TestB.Id });
				ba = archetypes.GetOrCreateArchetype(new[] { TestB.Id, TestA.Id });
				aOnly = archetypes.GetOrCreateArchetype(new[] { TestA.Id });
				aNoB = archetypes.GetOrCreateArchetype(new[] { TestA.Id }, new[] { TestB.Id });
			});
			Archetype fromSecondSystem = null;
			AddSystem(null, archetypes =>
				fromSecondSystem = archetypes.GetOrCreateArchetype(new[] { TestB.Id, TestA.Id }));

			Assert.AreSame(ab, ba);
			Assert.AreSame(ab, fromSecondSystem);
			Assert.AreNotSame(ab, aOnly);
			Assert.AreNotSame(aOnly, aNoB);
		}

		[Test]
		public void DestroyedEntity_LeavesArchetypes_AndReusedIdIsUnaffected()
		{
			var gameObject = CreateBaked("Old", a: 1);
			CreateWorld();
			var old = Entity.Null;
			var reused = Entity.Null;
			var frame = 0;
			Archetype noTag = null;
			Entity[] inNoTag = null;

			AddSystem(
				(componentManager, commandBuffer, world) =>
				{
					if (frame == 0)
					{
						commandBuffer.DestroyEntity(old);
					}
					else
					{
						commandBuffer.AddComponent(reused, new TestA());
					}
				},
				// Exclude-only: every entity with components and no TestTag
				archetypes => noTag = archetypes.GetOrCreateArchetype(Array.Empty<int>(), new[] { TestTag.Id }));
			AddSystem((componentManager, commandBuffer, world) =>
				inNoTag = componentManager.GetEntities(noTag).ToArray());
			World.Start();
			old = EntityOf(gameObject);

			World.Update(Dt);
			CollectionAssert.AreEqual(Array.Empty<Entity>(), inNoTag, "after destroy");

			reused = World.CreateEntity("Reused");
			Assert.AreEqual(old.Id, reused.Id);
			frame = 1;
			World.Update(Dt);
			CollectionAssert.AreEqual(new[] { reused }, inNoTag, "after the reused entity got a component");
		}
	}
}
