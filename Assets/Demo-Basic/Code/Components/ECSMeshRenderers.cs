using Beneton.ECS.Core;
using UnityEngine;

namespace ECS.Demo.Basic.Components
{
	/// <summary>
	/// Caches Unity <see cref="MeshRenderer"/> references for an entity, allowing ECS systems to modify visual properties.
	/// </summary>
	public partial struct EcsMeshRenderers : IComponent
	{
		public MeshRenderer[] MeshRenderers;
	}
}