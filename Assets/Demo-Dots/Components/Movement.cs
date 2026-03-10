using Unity.Entities;
using Unity.Mathematics;

namespace DotsDemo
{
	public struct Movement : IComponentData
	{
		public float Speed;
		public float3 Direction;
	}
}