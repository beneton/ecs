namespace Beneton.ECS.Core
{
	/// <summary>
	/// Marks a component as a singleton, ensuring that only one instance of this component type exists within the entire <see cref="World"/>.
	/// - Used for global state, configuration, or unique markers that should not be duplicated across multiple entities.
	/// - The <see cref="ComponentManager"/> enforces this constraint by preventing the addition of a singleton component if another entity already possesses it.
	/// </summary>
	public interface ISingletonComponent : IComponent
	{
	}
}