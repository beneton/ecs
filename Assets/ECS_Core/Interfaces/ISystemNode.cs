using System;
using UnityEngine;

namespace Beneton.ECS.Core
{
	public interface ISystemNode
	{
		Type SystemType { get; }

		GameObject GetGameObject();

		void EcsUpdate(
			float deltaTime,
			Entity entity,
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer,
			IWorld world);
	}

	public interface ISystemNode<T> : ISystemNode where T : BaseSystem
	{
		Type ISystemNode.SystemType => typeof(T);
	}
}