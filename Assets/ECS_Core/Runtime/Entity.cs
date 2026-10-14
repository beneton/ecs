using System;
using UnityEngine;

namespace Beneton.ECS.Core
{
	/// <summary>
	/// A handle to an entity in the <see cref="World"/>.
	/// - <see cref="Id"/> is the entity's slot. Slots are reused after an entity is destroyed,
	///   so every storage stays as small as the peak number of alive entities.
	/// - <see cref="Version"/> increases every time a slot is reused. A handle is only valid while its
	///   Version matches the slot's current one, so a handle to a destroyed entity never points at
	///   the new entity occupying its slot (see <see cref="World.HasEntity"/>).
	/// </summary>
	[Serializable]
	public struct Entity : IEquatable<Entity>
	{
		[SerializeField]
		private int _id;

		[SerializeField]
		private int _version;

		public int Id => _id;
		public int Version => _version;

		internal const int NullId = 0;
		internal const int NullVersion = 0;
		public static readonly Entity Null = new(NullId, NullVersion);

		public bool IsNull => _id == NullId;
		public bool IsNotNull => _id != NullId;

		public Entity(int id, int version)
		{
			_id = id;
			_version = version;
		}

		public bool Equals(Entity other)
		{
			return _id == other._id && _version == other._version;
		}

		public override bool Equals(object obj)
		{
			return obj is Entity other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(_id, _version);
		}

		public static bool operator ==(Entity a, Entity b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Entity a, Entity b)
		{
			return !a.Equals(b);
		}

		/// <summary>
		/// Converts to the slot <see cref="Id"/>, which is the key used by every <see cref="SparseSet{T}"/>.
		/// </summary>
		public static implicit operator int(Entity a)
		{
			return a._id;
		}

		public override string ToString()
		{
			return $"Entity ({_id.ToString()}:{_version.ToString()})";
		}
	}
}
