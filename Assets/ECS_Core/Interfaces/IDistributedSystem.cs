using System;

namespace Beneton.ECS.Core
{
	public interface IDistributedSystem
	{
		Type SystemType { get; }
		void UnregisterNode(Entity entity);
		void RegisterNode(ISystemNode node, Entity entity);
	}
}