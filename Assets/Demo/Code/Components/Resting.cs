using Beneton.ECS.Core;

namespace ECSSample.Components
{
	/// <summary>
	/// Indicates that an entity is currently resting and tracks the remaining duration of the resting state.
	/// </summary>
	public partial struct Resting : IComponent
	{
		public float Duration;
	}
}