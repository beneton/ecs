using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Beneton.ECS.Core.Tests
{
	public class CommandBufferTests : EcsTestBase
	{
		[Test]
		public void AddAndRemove_AreDeferredUntilTheSystemEnds()
		{
			CreateWorld();
			var entity = World.CreateEntity("E");
			var frame = 0;
			var seenByWriter = new List<bool>();
			var seenByNextSystem = new List<bool>();

			AddSystem((componentManager, commandBuffer, world) =>
			{
				if (frame == 0)
				{
					commandBuffer.AddComponent(entity, new TestA { Value = 1 });
				}
				else
				{
					commandBuffer.RemoveComponent<TestA>(entity);
				}

				seenByWriter.Add(componentManager.HasComponent<TestA>(entity));
			});
			AddSystem((componentManager, commandBuffer, world) =>
				seenByNextSystem.Add(componentManager.HasComponent<TestA>(entity)));
			World.Start();

			World.Update(Dt);
			frame++;
			World.Update(Dt);

			CollectionAssert.AreEqual(new[] { false, true }, seenByWriter);
			CollectionAssert.AreEqual(new[] { true, false }, seenByNextSystem);
		}

		[Test]
		public void UpdateComponent_WorksOnACopy_AndIsDeferred()
		{
			var gameObject = CreateBaked("E", a: 1);
			CreateWorld();
			var entity = Entity.Null;
			var seenByWriter = 0;
			var seenByNextSystem = 0;

			AddSystem((componentManager, commandBuffer, world) =>
			{
				var a = componentManager.GetComponent<TestA>(entity);
				a.Value = 2;
				commandBuffer.UpdateComponent(entity, a);
				seenByWriter = componentManager.GetComponent<TestA>(entity).Value;
			});
			AddSystem((componentManager, commandBuffer, world) =>
				seenByNextSystem = componentManager.GetComponent<TestA>(entity).Value);
			World.Start();
			entity = EntityOf(gameObject);

			World.Update(Dt);

			Assert.AreEqual(1, seenByWriter);
			Assert.AreEqual(2, seenByNextSystem);
		}

		[Test]
		public void AddOrUpdate_AndAddMissing()
		{
			var gameObject = CreateBaked("E", a: 1);
			CreateWorld();
			var entity = Entity.Null;
			TestA a = default;
			TestB b = default;
			var hasTag = false;

			AddSystem((componentManager, commandBuffer, world) =>
			{
				commandBuffer.AddOrUpdateComponent(entity, new TestA { Value = 2 }); // Present: updates
				commandBuffer.AddOrUpdateComponent(entity, new TestB { Value = 3 }); // Missing: adds
				commandBuffer.AddMissingComponent(entity, new TestA { Value = 4 }); // Present: ignored
				commandBuffer.AddMissingComponent(entity, new TestTag()); // Missing: adds
			});
			AddSystem((componentManager, commandBuffer, world) =>
			{
				a = componentManager.GetComponent<TestA>(entity);
				b = componentManager.GetComponent<TestB>(entity);
				hasTag = componentManager.HasComponent<TestTag>(entity);
			});
			World.Start();
			entity = EntityOf(gameObject);

			World.Update(Dt);

			Assert.AreEqual(2, a.Value);
			Assert.AreEqual(3, b.Value);
			Assert.IsTrue(hasTag);
		}

		[Test]
		public void Commands_ApplyInRecordedOrder()
		{
			CreateWorld();
			var entity = World.CreateEntity("E");
			var hasA = true;
			var hasB = false;

			AddSystem((componentManager, commandBuffer, world) =>
			{
				commandBuffer.AddComponent(entity, new TestA());
				commandBuffer.RemoveComponent<TestA>(entity);
				commandBuffer.RemoveComponent<TestB>(entity);
				commandBuffer.AddComponent(entity, new TestB());
			});
			AddSystem((componentManager, commandBuffer, world) =>
			{
				hasA = componentManager.HasComponent<TestA>(entity);
				hasB = componentManager.HasComponent<TestB>(entity);
			});
			World.Start();

			World.Update(Dt);

			Assert.IsFalse(hasA, "add then remove");
			Assert.IsTrue(hasB, "remove then add");
		}

		[Test]
		public void AddComponent_WhenAlreadyPresent_LogsErrorAndKeepsValue()
		{
			var gameObject = CreateBaked("E", a: 1);
			CreateWorld();
			var entity = Entity.Null;
			var value = 0;

			AddSystem((componentManager, commandBuffer, world) =>
				commandBuffer.AddComponent(entity, new TestA { Value = 2 }));
			AddSystem((componentManager, commandBuffer, world) =>
				value = componentManager.GetComponent<TestA>(entity).Value);
			World.Start();
			entity = EntityOf(gameObject);

			LogAssert.Expect(LogType.Error, new Regex("already has component"));
			World.Update(Dt);

			Assert.AreEqual(1, value);
		}

		[Test]
		public void CheckedCalls_RejectStaleHandle_EvenWhenItsIdIsReused()
		{
			CreateWorld();
			var stale = World.CreateEntity("Old");
			World.DestroyEntity(stale);
			var current = World.CreateEntity("New");
			Assert.AreEqual(stale.Id, current.Id);

			var frame = 0;
			var staleTryGet = true;
			var staleHas = true;
			TestA currentA = default;
			var currentHasB = true;

			AddSystem((componentManager, commandBuffer, world) =>
			{
				if (frame == 0)
				{
					commandBuffer.AddComponent(current, new TestA { Value = 2 });
					return;
				}

				if (frame == 1)
				{
					staleTryGet = componentManager.TryGetComponent<TestA>(stale, out _);
					staleHas = componentManager.HasComponent<TestA>(stale);
					commandBuffer.RemoveComponent<TestA>(stale);
					commandBuffer.AddComponent(stale, new TestB());
					return;
				}

				currentA = componentManager.GetComponent<TestA>(current);
				currentHasB = componentManager.HasComponent<TestB>(current);
			});
			World.Start();

			World.Update(Dt);

			// TryGet, Has, Remove and Add on the stale handle each report it
			for (var i = 0; i < 4; i++)
			{
				LogAssert.Expect(LogType.Error, new Regex("not part of the World"));
			}

			frame = 1;
			World.Update(Dt);
			frame = 2;
			World.Update(Dt);

			Assert.IsFalse(staleTryGet);
			Assert.IsFalse(staleHas);
			Assert.AreEqual(2, currentA.Value, "the new entity's TestA was not removed");
			Assert.IsFalse(currentHasB, "the new entity didn't get TestB");
		}
	}
}
