namespace Beneton.ECS.Core
{
	public interface IArchetypeProvider
	{
		Archetype GetOrCreateArchetype(int[] required);
		Archetype GetOrCreateArchetype(int[] required, int[] exclude);
	}
}