using Beneton.ECS.Core;
using UnityEngine;

namespace ECSSample.Components
{
	public partial struct ECSMeshRenderers : IComponent
	{
		public MeshRenderer[] MeshRenderers;
	}
}