using Unity.Entities;
using Unity.Mathematics;

namespace DotsDemo
{
	public struct Config : IComponentData
	{
		public Random Random;
	}
}