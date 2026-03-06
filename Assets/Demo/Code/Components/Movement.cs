using Beneton.ECS.Core;
using UnityEngine;

namespace ECSSample.Components
{
	public partial struct Movement : IComponent
	{
		public float Speed;
		public Vector3 Direction;

		public Transform Transform;
	}
}