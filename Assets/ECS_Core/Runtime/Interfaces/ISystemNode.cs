using System;
using UnityEngine;

namespace Beneton.ECS.Core
{
	/// <summary>
	/// Represents a logic node that can be integrated into a <see cref="DistributedSystem{T}"/>.
	/// - Typically implemented by Unity <see cref="MonoBehaviour"/>s to bridge Unity-specific events or data with the ECS.
	/// - Receives the <see cref="EcsUpdate"/> call from its parent system, providing access to the ECS context.
	/// </summary>
	public interface ISystemNode
	{
		/// <summary>
		/// Gets the type of the <see cref="BaseSystem"/> this node belongs to.
		/// </summary>
		Type SystemType { get; }

		/// <summary>
		/// Executes the system logic for this node.
		/// </summary>
		void EcsUpdate(
			float deltaTime,
			Entity entity,
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer,
			IWorld world);
	}

	/// <summary>
	/// A generic version of <see cref="ISystemNode"/> that explicitly binds the node to a specific <see cref="BaseSystem"/> type.
	/// Favor using this interface instead of the non-generic one. It will make the code easier to ready ;)
	/// </summary>
	/// <typeparam name="T">The type of <see cref="BaseSystem"/> that manages this node.</typeparam>
	public interface ISystemNode<T> : ISystemNode where T : BaseSystem
	{
		Type ISystemNode.SystemType => typeof(T);
	}
}