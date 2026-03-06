namespace Beneton.ECS.Core
{
	public class AddComponentCommand<T> : ICommand where T : struct, IComponent
	{
		private Entity _entity;
		private T _component;

		private static readonly CommandPool<AddComponentCommand<T>> Pool =
			new(() => new AddComponentCommand<T>());

		public static AddComponentCommand<T> Get(Entity entity, T component)
		{
			var newCommand = Pool.Rent();
			newCommand._entity = entity;
			newCommand._component = component;
			return newCommand;
		}

		public void Execute(ComponentManager componentManager)
		{
			componentManager.AddComponent(_entity, _component);

			_entity = Entity.Null;
			_component = default;
			Pool.Return(this);
		}
	}
}