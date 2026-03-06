namespace Beneton.ECS.Core
{
	public class AddOrUpdateComponentCommand<T> : ICommand where T : struct, IComponent
	{
		private Entity _entity;
		private T _component;

		private static readonly CommandPool<AddOrUpdateComponentCommand<T>> Pool =
			new(() => new AddOrUpdateComponentCommand<T>());

		public static AddOrUpdateComponentCommand<T> Get(Entity entity, T component)
		{
			var newCommand = Pool.Rent();
			newCommand._entity = entity;
			newCommand._component = component;
			return newCommand;
		}

		public void Execute(ComponentManager componentManager)
		{
			componentManager.AddOrUpdateComponent(_entity, _component);

			_entity = Entity.Null;
			_component = default;
			Pool.Return(this);
		}
	}
}