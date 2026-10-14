using NUnit.Framework;
using UnityEngine;

namespace Beneton.ECS.Core.Tests
{
	public class SpawnTests : EcsTestBase
	{
		// A scene GameObject works as a prefab for Instantiate. Create it after World.Start,
		// otherwise Start turns the template itself into an entity.
		private static GameObject CreateTemplate(bool rootBaker)
		{
			var root = rootBaker ? CreateBaked("Template", a: 1) : new GameObject("Template");
			var child = CreateBaked("Child", b: 2, parent: root.transform);
			new GameObject("Plain").transform.SetParent(child.transform);
			return root;
		}

		[Test]
		public void Spawn_RootAndChildBecomeSeparateEntities()
		{
			CreateWorld();
			World.Start();
			var template = CreateTemplate(rootBaker: true);

			var result = World.Spawn(template, null);

			Assert.AreNotSame(template, result.GameObject);
			Assert.AreEqual("Template", result.GameObject.name.Split(' ')[0], "(Clone) is stripped");
			Assert.IsTrue(World.HasEntity(result.Entity));
			Assert.AreEqual(result.Entity, EntityOf(result.GameObject));

			Assert.AreEqual(1, result.Children.Count, "only GameObjects with Bakers or nodes are entities");
			var (childEntity, childGameObject) = result.Children[0];
			Assert.IsTrue(childGameObject.name.StartsWith("Child"));
			Assert.AreNotEqual(result.Entity, childEntity);
			Assert.AreEqual(childEntity, EntityOf(childGameObject));
		}

		[Test]
		public void Spawn_RootWithoutBaker_GivesNullEntityButStillSpawnsChildren()
		{
			CreateWorld();
			World.Start();
			var template = CreateTemplate(rootBaker: false);

			var result = World.Spawn(template, null);

			Assert.IsTrue(result.Entity.IsNull);
			Assert.IsNotNull(result.GameObject);
			Assert.AreEqual(1, result.Children.Count);
			Assert.IsTrue(World.HasEntity(result.Children[0].Entity));
		}

		[Test]
		public void Spawn_NoChildEntities_GivesEmptyChildren()
		{
			CreateWorld();
			World.Start();
			var template = CreateBaked("Template", a: 1);

			var result = World.Spawn(template, null);

			Assert.IsNotNull(result.Children);
			Assert.AreEqual(0, result.Children.Count);
		}

		[Test]
		public void SpawnFromASystem_IsBakedAndVisibleToLaterSystems()
		{
			CreateWorld();
			GameObject template = null;
			SpawnResult spawned = default;
			Archetype withA = null;
			Entity[] inA = null;
			int a = 0, childB = 0;

			AddSystem((componentManager, commandBuffer, world) => spawned = world.Spawn(template, null));
			AddSystem(
				(componentManager, commandBuffer, world) =>
				{
					inA = componentManager.GetEntities(withA).ToArray();
					a = componentManager.GetComponent<TestA>(spawned.Entity).Value;
					childB = componentManager.GetComponent<TestB>(spawned.Children[0].Entity).Value;
				},
				archetypes => withA = archetypes.GetOrCreateArchetype(new[] { TestA.Id }));
			World.Start();
			template = CreateTemplate(rootBaker: true);

			World.Update(Dt);

			CollectionAssert.AreEqual(new[] { spawned.Entity }, inA);
			Assert.AreEqual(1, a);
			Assert.AreEqual(2, childB);
		}
	}
}
