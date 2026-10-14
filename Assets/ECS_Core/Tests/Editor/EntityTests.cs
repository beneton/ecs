using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Beneton.ECS.Core.Tests
{
	public class EntityTests : EcsTestBase
	{
		[Test]
		public void Equality_UsesIdAndVersion()
		{
			Assert.AreEqual(Entity.Null, default(Entity));
			Assert.IsTrue(default(Entity).IsNull);
			Assert.AreEqual(0, Entity.Null.Id);
			Assert.AreEqual(0, Entity.Null.Version);

			Assert.IsTrue(new Entity(3, 1) == new Entity(3, 1));
			Assert.IsTrue(new Entity(3, 1) != new Entity(3, 2));
			Assert.IsTrue(new Entity(3, 1) != new Entity(4, 1));
			Assert.AreEqual(2, new HashSet<Entity> { new(3, 1), new(3, 2) }.Count);

			int id = new Entity(3, 2);
			Assert.AreEqual(3, id);
		}

		[Test]
		public void CreateEntity_GivesDistinctLiveEntitiesAtVersionOne()
		{
			CreateWorld();
			var a = World.CreateEntity("A");
			var b = World.CreateEntity("B");

			Assert.AreNotEqual(a.Id, b.Id);
			Assert.IsTrue(a.IsNotNull);
			Assert.AreEqual(1, a.Version);
			Assert.AreEqual(1, b.Version);
			Assert.IsTrue(World.HasEntity(a));
			Assert.IsTrue(World.HasEntity(b));

			// The tracking GameObject maps back to the same entity
			Assert.IsTrue(World.TryGetGameObject(a, out var gameObject));
			Assert.AreEqual(a, World.GetOrCreateEntity(gameObject));
			Assert.IsTrue(World.TryGetEntity(gameObject, out var found));
			Assert.AreEqual(a, found);
		}

		[Test]
		public void DestroyEntity_FreesIdForReuseWithNextVersion()
		{
			CreateWorld();
			var old = World.CreateEntity("Old");
			World.DestroyEntity(old);
			var reused = World.CreateEntity("Reused");

			Assert.AreEqual(old.Id, reused.Id);
			Assert.AreEqual(old.Version + 1, reused.Version);
			Assert.IsFalse(World.HasEntity(old));
			Assert.IsTrue(World.HasEntity(reused));
		}

		[Test]
		public void DestroyEntity_Twice_IsANoOp()
		{
			CreateWorld();
			var entity = World.CreateEntity("E");
			World.DestroyEntity(entity);
			World.DestroyEntity(entity);

			// If the second destroy had freed the slot again, both would reuse the same Id
			var first = World.CreateEntity("First");
			var second = World.CreateEntity("Second");
			Assert.AreEqual(entity.Id, first.Id);
			Assert.AreNotEqual(entity.Id, second.Id);
		}

		[Test]
		public void TryGetGameObject_RejectsStaleHandle()
		{
			CreateWorld();
			var old = World.CreateEntity("Old");
			World.DestroyEntity(old);
			var reused = World.CreateEntity("Reused");

			Assert.IsFalse(World.TryGetGameObject(old, out var staleGameObject));
			Assert.IsNull(staleGameObject);

			Assert.IsTrue(World.TryGetGameObject(reused, out var gameObject));
			Assert.IsTrue(gameObject.name.StartsWith("Reused"));
		}
	}
}
