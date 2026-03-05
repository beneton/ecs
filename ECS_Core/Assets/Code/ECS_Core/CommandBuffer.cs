namespace Beneton.ECS.Core
{
	public class CommandBuffer<TSystem> : ICommandBuffer where TSystem : BaseSystem
	{
		private readonly SparseSet<ICommand> _commands = new();
		private int _commandId = 0;
		private bool _shouldUpdateArchetypes = false;

		public void AddComponent<T>(Entity entity, T component) where T : struct, IComponent
		{
			var command = AddComponentCommand<T>.Get(entity, component);
			_commands.Set(_commandId++, command);
			_commandId++;
			_shouldUpdateArchetypes = true;
		}

		public void UpdateComponent<T>(Entity entity, T component) where T : struct, IComponent
		{
			var command = UpdateComponentCommand<T>.Get(entity, component);
			_commands.Set(_commandId++, command);
			_commandId++;
		}

		public void AddOrUpdateComponent<T>(Entity entity, T component) where T : struct, IComponent
		{
			var command = AddOrUpdateComponentCommand<T>.Get(entity, component);
			_commands.Set(_commandId++, command);
			_commandId++;
			_shouldUpdateArchetypes = true;
		}

		public void AddMissingComponent<T>(Entity entity, T component) where T : struct, IComponent
		{
			var command = AddMissingComponentCommand<T>.Get(entity, component);
			_commands.Set(_commandId++, command);
			_commandId++;
			_shouldUpdateArchetypes = true;
		}

		public void RemoveComponent<T>(Entity entity) where T : struct, IComponent
		{
			var command = RemoveComponentCommand<T>.Get(entity);
			_commands.Set(_commandId++, command);
			_commandId++;
			_shouldUpdateArchetypes = true;
		}

		public void Execute(ComponentManager componentManager)
		{
			foreach (var command in _commands.Values)
			{
				command.Execute(componentManager);
			}

			_commands.Clear();
			_commandId = 0;

			if (_shouldUpdateArchetypes)
			{
				componentManager.UpdateArchetypes();
			}

			_shouldUpdateArchetypes = false;
		}
	}
}