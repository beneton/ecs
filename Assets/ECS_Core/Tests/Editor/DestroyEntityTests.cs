using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Beneton.ECS.Core.Tests
{
	public class DestroyEntityTests : EcsTestBase
	{
		[Test]
		public void CommandBufferDestroy_IsDeferredUntilTheSystemEnds()
		{
			var gameObject = CreateBaked("E", a: 1);
			CreateWorld();
			var entity = Entity.Null;
			var aliveForWriter = false;
			var componentForWriter = false;
			var aliveForNextSystem = true;
			Archetype withA = null;
			Entity[] inA = null;

			AddSystem((componentManager, commandBuffer, world) =>
			{
				commandBuffer.DestroyEntity(entity);
				aliveForWriter = world.HasEntity(entity);
				componentForWriter = componentManager.TryGetComponent<TestA>(entity, out _);
			});
			AddSystem(
				(componentManager, commandBuffer, world) =>
				{
					aliveForNextSystem = world.HasEntity(entity);
					inA = componentManager.GetEntities(withA).ToArray();
				},
				archetypes => withA = archetypes.GetOrCreateArchetype(new[] { TestA.Id }));
			World.Start();
			entity = EntityOf(gameObject);

			World.Update(Dt);

			Assert.IsTrue(aliveForWriter);
			Assert.IsTrue(componentForWriter);
			Assert.IsFalse(aliveForNextSystem);
			CollectionAssert.IsEmpty(inA);
		}

		[Test]
		public void DestroyingDuringIteration_VisitsEveryEntityExactlyOnce()
		{
			CreateBaked("E0", a: 0);
			CreateBaked("E1", a: 1);
			CreateBaked("E2", a: 2);
			CreateWorld();
			var frame = 0;
			var destroyed = Entity.Null;
			var initial = new List<Entity>();
			var visits = new List<Entity>();
			Archetype withA = null;

			AddSystem(
				(componentManager, commandBuffer, world) =>
				{
					var entities = componentManager.GetEntities(withA);
					if (frame == 0)
					{
						initial.AddRange(entities.ToArray());
						destroyed = entities[entities.Length - 1];
					}

					foreach (var entity in entities)
					{
						visits.Add(entity);
						if (frame == 0 && visits.Count == 1)
						{
							// Destroy the last entity while iterating the first
							commandBuffer.DestroyEntity(destroyed);
						}
					}
				},
				archetypes => withA = archetypes.GetOrCreateArchetype(new[] { TestA.Id }));
			World.Start();

			World.Update(Dt);
			Assert.AreEqual(3, initial.Count);
			CollectionAssert.AreEquivalent(initial, visits, "frame 0 visits");

			visits.Clear();
			frame = 1;
			World.Update(Dt);
			CollectionAssert.AreEquivalent(initial.Where(e => e != destroyed), visits, "frame 1 visits");
		}

		[Test]
		public void DestroyEntity_KeepsTheGameObject()
		{
			CreateWorld();
			var entity = World.CreateEntity("Tracked");
			Assert.IsTrue(World.TryGetGameObject(entity, out var gameObject));

			World.DestroyEntity(entity);

			Assert.IsTrue(gameObject != null, "the GameObject was destroyed");
			Assert.IsFalse(World.TryGetGameObject(entity, out _));
			Assert.IsFalse(World.TryGetEntity(gameObject, out _));
		}
	}
}
