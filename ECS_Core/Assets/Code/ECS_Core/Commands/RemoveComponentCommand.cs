namespace Beneton.ECS.Core
{
	public class RemoveComponentCommand<T> : ICommand where T : struct, IComponent
	{
		private Entity _entity;

		private static readonly CommandPool<RemoveComponentCommand<T>> Pool =
			new(() => new RemoveComponentCommand<T>());

		public static RemoveComponentCommand<T> Get(Entity entity)
		{
			var newCommand = Pool.Rent();
			newCommand._entity = entity;
			return newCommand;
		}


		public void Execute(ComponentManager componentManager)
		{
			componentManager.RemoveComponent<T>(_entity);

			_entity = Entity.Null;
			Pool.Return(this);
		}
	}
}