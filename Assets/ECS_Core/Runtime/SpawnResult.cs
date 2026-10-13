using System.Collections.Generic;
using UnityEngine;

namespace Beneton.ECS.Core
{
	/// <summary>
	/// The result of <see cref="IWorld.Spawn"/>.
	/// - <see cref="Entity"/> / <see cref="GameObject"/>: the instantiated root. <see cref="Entity"/> is
	///   <see cref="Entity.Null"/> when the root has no <see cref="Baker"/> or <see cref="ISystemNode"/>.
	/// - <see cref="Children"/>: every other GameObject in the instance that became an entity. Empty when there are none.
	/// </summary>
	public readonly struct SpawnResult
	{
		// Shared by every result without children. Exposed only as IReadOnlyList so it can't be modified.
		private static readonly List<(Entity Entity, GameObject GameObject)> NoChildren = new(0);

		public readonly Entity Entity;
		public readonly GameObject GameObject;
		public readonly IReadOnlyList<(Entity Entity, GameObject GameObject)> Children;

		public SpawnResult(
			Entity entity,
			GameObject gameObject,
			IReadOnlyList<(Entity Entity, GameObject GameObject)> children)
		{
			Entity = entity;
			GameObject = gameObject;
			Children = children ?? NoChildren;
		}
	}
}