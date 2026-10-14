using NUnit.Framework;

namespace Beneton.ECS.Core.Tests
{
	public class SparseSetTests
	{
		[Test]
		public void SetHasGetTryGet_RoundTrip()
		{
			var set = new SparseSet<string>();
			set.Set(3, "three");
			set.Set(10, "ten");

			Assert.AreEqual(2, set.Length);
			Assert.IsTrue(set.Has(3));
			Assert.AreEqual("ten", set.Get(10));

			Assert.AreEqual("three", set.TryGet(3, out var exists));
			Assert.IsTrue(exists);

			Assert.IsNull(set.TryGet(4, out exists));
			Assert.IsFalse(exists);

			// Beyond the current capacity
			Assert.IsFalse(set.Has(5000));
		}

		[Test]
		public void Set_ExistingKey_OverwritesWithoutGrowing()
		{
			var set = new SparseSet<string>();
			set.Set(5, "a");
			set.Set(5, "b");

			Assert.AreEqual(1, set.Length);
			Assert.AreEqual("b", set.Get(5));
		}

		[Test]
		public void Remove_MovesLastIntoGap_AndKeysStayAligned()
		{
			var set = new SparseSet<int>();
			for (var key = 1; key <= 5; key++)
			{
				set.Set(key, key * 10);
			}

			set.Remove(2);
			set.Remove(2); // Already removed: no-op
			set.Remove(9999); // Never added, beyond capacity: no-op

			Assert.AreEqual(4, set.Length);
			Assert.IsFalse(set.Has(2));
			foreach (var key in new[] { 1, 3, 4, 5 })
			{
				Assert.AreEqual(key * 10, set.Get(key), $"key {key}");
			}

			for (var i = 0; i < set.Length; i++)
			{
				Assert.AreEqual(set.Keys[i] * 10, set.Values[i], $"dense index {i}");
			}

			set.Clear();
			Assert.AreEqual(0, set.Length);
			Assert.IsFalse(set.Has(1));
		}

		[Test]
		public void Set_BeyondCapacity_GrowsAndKeepsValues()
		{
			var set = new SparseSet<int>(2);
			for (var key = 0; key < 100; key++)
			{
				set.Set(key, key);
			}

			set.Set(1000, 1000);

			Assert.AreEqual(101, set.Length);
			for (var key = 0; key < 100; key++)
			{
				Assert.AreEqual(key, set.Get(key));
			}

			Assert.AreEqual(1000, set.Get(1000));
		}
	}
}
