using System.Collections.Generic;
using NUnit.Framework;

namespace Beneton.ECS.Core.Tests
{
	public class SystemOrderTests : EcsTestBase
	{
		[Test]
		public void Systems_RunInRegistrationOrder_LateOnesOnlyInLateUpdate()
		{
			CreateWorld();
			var log = new List<string>();

			AddSystem((componentManager, commandBuffer, world) => log.Add("A"));
			AddSystemLate((componentManager, commandBuffer, world) => log.Add("Late1"));
			AddSystem((componentManager, commandBuffer, world) => log.Add("B"));
			AddSystemLate((componentManager, commandBuffer, world) => log.Add("Late2"));
			World.Start();

			World.Update(Dt);
			CollectionAssert.AreEqual(new[] { "A", "B" }, log);

			World.LateUpdate(Dt);
			CollectionAssert.AreEqual(new[] { "A", "B", "Late1", "Late2" }, log);
		}

		[Test]
		public void LateSystem_CommandBufferIsAppliedBeforeTheNextLateSystem()
		{
			CreateWorld();
			var entity = World.CreateEntity("E");
			var seen = false;

			AddSystemLate((componentManager, commandBuffer, world) =>
				commandBuffer.AddComponent(entity, new TestA()));
			AddSystemLate((componentManager, commandBuffer, world) =>
				seen = componentManager.HasComponent<TestA>(entity));
			World.Start();

			RunFrame();

			Assert.IsTrue(seen);
		}
	}
}
