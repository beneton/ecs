using System;

namespace Beneton.ECS.Core
{
	/// <summary>
	/// Defines the contract for an ECS system that distributes its update logic among multiple <see cref="ISystemNode"/>s.
	/// - Provides mechanisms for registering and unregistering logic nodes tied to specific entities.
	/// - Enables decoupling of system logic from a single centralized update call.
	/// </summary>
	public interface IDistributedSystem
	{
		/// <summary>
		/// Gets the type of the concrete <see cref="BaseSystem"/> that this interface represents.
		/// </summary>
		Type SystemType { get; }

		/// <summary>
		/// Removes a registered node associated with the specified entity.
		/// </summary>
		/// <param name="entity">The entity whose associated node should be unregistered.</param>
		void UnregisterNode(Entity entity);

		/// <summary>
		/// Registers a new logic node associated with a specific entity.
		/// </summary>
		/// <param name="node">The node implementation containing the logic.</param>
		/// <param name="entity">The entity the node is associated with.</param>
		void RegisterNode(ISystemNode node, Entity entity);
	}
}