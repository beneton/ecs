namespace Beneton.ECS.Core
{
	public interface ICommand
	{
		void Execute(ComponentManager componentManager);
	}
}