using System;

namespace Beneton.ECS.Core
{
	/// <summary>
	/// Provides a high-performance object pooling mechanism for command objects.
	/// - Reduces memory allocations and garbage collection pressure by reusing command instances.
	/// - Implements <see cref="Rent"/> and <see cref="Return"/> methods for efficient object lifecycle management.
	/// - Automatically expands its internal storage capacity when the pool is exhausted.
	/// </summary>
	public sealed class CommandPool<T> where T : class
	{
		private T[] _items;
		private readonly Func<T> _factory;

		private int _availableCount;

		public CommandPool(Func<T> factory, int initialCapacity = 8)
		{
			_items = new T[initialCapacity];
			_factory = factory;
			_availableCount = 0;
		}

		public T Rent()
		{
			if (_availableCount == 0)
			{
				return _factory();
			}

			_availableCount--;
			var item = _items[_availableCount];
			_items[_availableCount] = null;
			return item;
		}

		public void Return(T item)
		{
			if (_availableCount == _items.Length)
			{
				Expand();
			}

			_items[_availableCount] = item;
			_availableCount++;
		}

		private void Expand()
		{
			var newSize = _items.Length * 2;
			var newArray = new T[newSize];

			Array.Copy(_items, newArray, _items.Length);

			_items = newArray;
		}
	}
}