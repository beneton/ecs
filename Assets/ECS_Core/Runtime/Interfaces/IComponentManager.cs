namespace Beneton.ECS.Core
{
	/// <summary>
	/// Aggregates both component modification and retrieval interfaces into a single contract.
	/// - The <see cref="ComponentManager"/> is split into <see cref="IComponentModifier"/> and <see cref="IComponentGetter"/> to enforce architectural constraints.
	/// - For example, <see cref="BaseSystem"/> only receives <see cref="IComponentGetter"/> to ensure that all structural modifications are deferred through a <see cref="ICommandBuffer"/>.
	/// - Conversely, <see cref="Baker"/>s receive the full <see cref="IComponentManager"/> as they are responsible for the initial entity setup where direct modifications are necessary.
	/// </summary>
	public interface IComponentManager : IComponentModifier, IComponentGetter
	{
	}
}