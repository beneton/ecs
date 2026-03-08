namespace Beneton.ECS.Core
{
	/// <summary>
	/// Defines the interface for managing and providing <see cref="Archetype"/>s.
	/// - Part of the <see cref="ComponentManager"/>'s responsibility split.
	/// - Provides methods for creating or retrieving unique archetypes based on component requirements.
	/// - Used by <see cref="BaseSystem.OnCreate"/> to initialize system queries.
	/// </summary>
	public interface IArchetypeProvider
	{
		/// <summary>
		/// Gets or creates an archetype with the specified required component types.
		/// </summary>
		/// <param name="required">The IDs of the components an entity must have to match this archetype.</param>
		/// <returns>A unique <see cref="Archetype"/> instance.</returns>
		Archetype GetOrCreateArchetype(int[] required);

		/// <summary>
		/// Gets or creates an archetype with the specified required and excluded component types.
		/// </summary>
		/// <param name="required">The IDs of the components an entity must have to match this archetype.</param>
		/// <param name="exclude">The IDs of the components an entity must NOT have to match this archetype.</param>
		/// <returns>A unique <see cref="Archetype"/> instance.</returns>
		Archetype GetOrCreateArchetype(int[] required, int[] exclude);
	}
}