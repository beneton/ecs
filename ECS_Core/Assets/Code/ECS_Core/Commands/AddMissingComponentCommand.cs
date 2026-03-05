namespace Beneton.ECS.Core
{
	public class AddMissingComponentCommand<T> : ICommand where T : struct, IComponent
	{
		private Entity _entity;
		private T _component;

		private static readonly CommandPool<AddMissingComponentCommand<T>> Pool =
			new(() => new AddMissingComponentCommand<T>());

		public static AddMissingComponentCommand<T> Get(Entity entity, T component)
		{
			var newCommand = Pool.Rent();
			newCommand._entity = entity;
			newCommand._component = component;
			return newCommand;
		}

		public void Execute(ComponentManager componentManager)
		{
			componentManager.AddMissingComponent(_entity, _component);

			_entity = Entity.Null;
			_component = default;
			Pool.Return(this);
		}
	}
}