using Beneton.ECS.Core;

namespace ECSSample.Components
{
	/// <summary>
	/// Indicates that an entity is currently moving and tracks the duration of the current movement state.
	/// </summary>
	public partial struct Moving : IComponent
	{
		public float Duration;
	}
}