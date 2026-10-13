namespace Beneton.ECS.Core
{
	public class DestroyEntityCommand : ICommand
	{
		private Entity _entity;

		private static readonly CommandPool<DestroyEntityCommand> Pool =
			new(() => new DestroyEntityCommand());

		public static DestroyEntityCommand Get(Entity entity)
		{
			var newCommand = Pool.Rent();
			newCommand._entity = entity;
			return newCommand;
		}

		public void Execute(World world, ComponentManager componentManager)
		{
			world.DestroyEntity(_entity);

			_entity = Entity.Null;
			Pool.Return(this);
		}
	}
}