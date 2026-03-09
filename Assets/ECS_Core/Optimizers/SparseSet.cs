using System;

namespace Beneton.ECS.Core
{
	/// <summary>
	/// Provides a high-performance alternative to Dictionaries/HashSets for mapping integer keys to values of type <typeparamref name="T"/>.
	/// - Mechanism: Uses a dual-array approach with a "sparse" array (mapping keys to dense indices) and "dense" arrays (storing keys and values contiguously).
	/// - Speed: Offers O(1) complexity for insertion, removal, and lookup, and is cache-friendly for iterations as values are stored contiguously in memory.
	/// - Drawbacks: Can be memory-intensive, as the sparse array's size is proportional to the highest key value rather than the number of stored elements.
	/// </summary>
	public sealed class SparseSet<T>
	{
		private int[] _sparse; // maps key → dense index
		private int[] _dense; // maps dense index → value
		private T[] _values; // value storage
		private int _count;

		private static T Null = default;

		public int Length => _count;
		public ReadOnlySpan<int> Keys => new(_dense, 0, _count);
		public ReadOnlySpan<T> Values => new(_values, 0, _count);

		public SparseSet(int capacity = 128)
		{
			_sparse = new int[capacity];
			_dense = new int[capacity];
			_values = new T[capacity];
			_count = 0;
		}

		public void Set(int key, in T value)
		{
			if (Has(key))
			{
				_values[_sparse[key]] = value;
				return;
			}

			EnsureCapacity(key);

			var index = _count++;
			_sparse[key] = index;
			_dense[index] = key;
			_values[index] = value;
		}

		public bool Has(int key)
		{
			if (key >= _sparse.Length)
			{
				return false;
			}

			var index = _sparse[key];
			return index < _count && _dense[index] == key;
		}

		public ref T Get(int key)
		{
			return ref _values[_sparse[key]];
		}

		public ref T TryGet(int key, out bool exists)
		{
			exists = Has(key);
			if (!exists)
			{
				return ref Null;
			}

			return ref _values[_sparse[key]];
		}

		public void Remove(int key)
		{
			if (!Has(key))
			{
				return;
			}

			var index = _sparse[key];
			var last = _count - 1;

			// Move last element into removed slot
			var lastElement = _dense[last];
			_dense[index] = lastElement;
			_values[index] = _values[last];
			_sparse[lastElement] = index;

			_count--;
		}

		public void Clear(bool purge = false)
		{
			_count = 0;
			if (purge)
			{
				for (var i = 0; i < _values.Length; i++)
				{
					_values[i] = Null;
				}
			}
		}

		private void EnsureCapacity(int key)
		{
			var needed = key + 1;

			if (needed <= _sparse.Length)
			{
				return;
			}

			var newSize = _sparse.Length;
			while (newSize < needed)
			{
				newSize *= 2;
			}

			Array.Resize(ref _sparse, newSize);
			Array.Resize(ref _dense, newSize);
			Array.Resize(ref _values, newSize);
		}
	}
}