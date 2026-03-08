using Beneton.ECS.Core;
using UnityEngine;

namespace ECSSample.Components
{
	/// <summary>
	/// Contains core movement data, including speed, direction, and a reference to the entity's Unity <see cref="Transform"/>.
	/// </summary>
	public partial struct Movement : IComponent
	{
		public float Speed;
		public Vector3 Direction;

		public Transform Transform;
	}
}