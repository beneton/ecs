using Beneton.ECS.Core;

namespace ECS.Demo.Basic.Components
{
	/// <summary>
	/// Represents a UI button in the ECS that triggers the spawning of multiple traveler entities.
	/// </summary>
	public partial struct AddTravelerButton : IComponent
	{
		public int Amount;
	}
}