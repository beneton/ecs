using Unity.Entities;
using UnityEngine;

namespace DotsDemo
{
	public class ConfigManaged : IComponentData
	{
		public Material MovingMaterial;
		public Material RestingMaterial;
	}
}