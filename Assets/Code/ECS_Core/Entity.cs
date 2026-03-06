using System;
using UnityEngine;

namespace Beneton.ECS.Core
{
	[Serializable]
	public struct Entity : IEquatable<Entity>
	{
		private readonly int _id;

		[SerializeField]
		internal string IdString;

		internal const int NullId = 0;
		public static readonly Entity Null = new(NullId);

		public bool IsNull => _id == NullId;
		public bool IsNotNull => _id != NullId;

		public Entity(int id)
		{
			_id = id;
			IdString = _id.ToString();
		}

		public bool Equals(Entity other)
		{
			return _id == other._id;
		}

		public override bool Equals(object obj)
		{
			return obj is Entity other && Equals(other);
		}

		public override int GetHashCode()
		{
			return _id;
		}

		public static bool operator ==(Entity a, Entity b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Entity a, Entity b)
		{
			return !a.Equals(b);
		}

		public static implicit operator int(Entity a)
		{
			return a._id;
		}

		public override string ToString()
		{
			return $"Entity ({IdString})";
		}
	}
}