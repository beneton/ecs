using UnityEngine;

namespace Beneton.ECS.Core
{
	/// <summary>
	/// Serves as the base class for all Bakers in the ECS.
	/// - Used to convert Unity GameObjects and their components into ECS entities and components.
	/// - Provides an abstract <see cref="Bake"/> method implemented by specific bakers to define how data is transferred.
	/// </summary>
	[SelectionBase]
	public abstract class Baker : MonoBehaviour
	{
		protected internal abstract void Bake(
			Entity entity,
			IComponentManager componentManager,
			IWorld world);
	}
}