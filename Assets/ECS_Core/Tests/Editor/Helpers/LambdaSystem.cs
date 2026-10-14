using System;

namespace Beneton.ECS.Core.Tests
{
	/// <summary>
	/// A system whose OnCreate and Update are given as lambdas, so each test can declare its systems inline.
	/// </summary>
	public class LambdaSystem : BaseSystem
	{
		public delegate void UpdateAction(
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer,
			IWorld world);

		private readonly Action<IArchetypeProvider> _onCreate;
		private readonly UpdateAction _onUpdate;

		public LambdaSystem(UpdateAction onUpdate, Action<IArchetypeProvider> onCreate = null)
		{
			_onUpdate = onUpdate;
			_onCreate = onCreate;
		}

		public override void OnCreate(IArchetypeProvider archetypeProvider)
		{
			_onCreate?.Invoke(archetypeProvider);
		}

		public override void Update(
			float deltaTime,
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer,
			IWorld world)
		{
			_onUpdate?.Invoke(componentManager, commandBuffer, world);
		}
	}
}
