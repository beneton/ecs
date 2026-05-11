namespace Beneton.ECS.Core
{
	/// <summary>
	/// Defines the base contract for a deferred ECS operation.
	/// - Used by the <see cref="CommandBuffer{TOwner}"/> to store and later execute component modifications.
	/// - Encapsulates the specific logic for adding, updating, or removing components in its <see cref="Execute"/> method.
	/// </summary>
	public interface ICommand
	{
		/// <summary>
		/// Executes the specific ECS operation using the provided <see cref="ComponentManager"/>.
		/// </summary>
		/// <param name="componentManager">The manager used to apply structural changes.</param>
		void Execute(ComponentManager componentManager);
	}
}