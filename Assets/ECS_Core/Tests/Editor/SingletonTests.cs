using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Beneton.ECS.Core.Tests
{
	public class SingletonTests : EcsTestBase
	{
		[Test]
		public void TryGetSingleton_ReturnsOwnerWithItsVersion()
		{
			CreateWorld();

			// Make the owner a reused slot, so its Version is 2
			var old = World.CreateEntity("Old");
			World.DestroyEntity(old);
			var owner = World.CreateEntity("Owner");
			Assert.AreEqual(2, owner.Version);

			var foundBefore = true;
			var foundAfter = false;
			var foundEntity = Entity.Null;
			TestSingleton foundComponent = default;

			AddSystem((componentManager, commandBuffer, world) =>
				foundBefore = componentManager.TryGetSingleton<TestSingleton>(out _, out _));
			AddSystem((componentManager, commandBuffer, world) =>
				commandBuffer.AddComponent(owner, new TestSingleton { Value = 7 }));
			AddSystem((componentManager, commandBuffer, world) =>
				foundAfter = componentManager.TryGetSingleton(out foundEntity, out foundComponent));
			World.Start();

			World.Update(Dt);

			Assert.IsFalse(foundBefore);
			Assert.IsTrue(foundAfter);
			Assert.AreEqual(owner, foundEntity);
			Assert.AreEqual(7, foundComponent.Value);
		}

		[Test]
		public void AddingSingletonToSecondEntity_LogsErrorAndKeepsFirstOwner()
		{
			CreateWorld();
			var first = World.CreateEntity("First");
			var second = World.CreateEntity("Second");
			var owner = Entity.Null;
			var secondHasIt = true;

			AddSystem((componentManager, commandBuffer, world) =>
			{
				commandBuffer.AddComponent(first, new TestSingleton { Value = 1 });
				commandBuffer.AddComponent(second, new TestSingleton { Value = 2 });
			});
			AddSystem((componentManager, commandBuffer, world) =>
			{
				componentManager.TryGetSingleton<TestSingleton>(out owner, out _);
				secondHasIt = componentManager.HasComponent<TestSingleton>(second);
			});
			World.Start();

			LogAssert.Expect(LogType.Error, new Regex("Singleton Component"));
			World.Update(Dt);

			Assert.AreEqual(first, owner);
			Assert.IsFalse(secondHasIt);
		}
	}
}
