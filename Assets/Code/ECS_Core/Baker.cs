using UnityEngine;

namespace Beneton.ECS.Core
{
	[SelectionBase]
	public abstract class Baker : MonoBehaviour
	{
		protected internal abstract void Bake(
			Entity entity,
			IComponentManager componentManager,
			IWorld world);
	}
}