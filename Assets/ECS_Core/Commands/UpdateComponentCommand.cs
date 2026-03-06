namespace Beneton.ECS.Core
{
	public class UpdateComponentCommand<T> : ICommand where T : struct, IComponent
	{
		private Entity _entity;
		private T _component;

		private static readonly CommandPool<UpdateComponentCommand<T>> Pool =
			new(() => new UpdateComponentCommand<T>());

		public static UpdateComponentCommand<T> Get(Entity entity, T component)
		{
			var newCommand = Pool.Rent();
			newCommand._entity = entity;
			newCommand._component = component;
			return newCommand;
		}

		public void Execute(ComponentManager componentManager)
		{
			componentManager.UpdateComponent(_entity, _component);

			_entity = Entity.Null;
			_component = default;
			Pool.Return(this);
		}
	}
}