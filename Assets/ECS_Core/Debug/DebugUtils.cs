#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;

namespace Beneton.ECS.Core.Editor
{
	/// <summary>
	/// Provides static utility methods for Editor-only debugging and inspection of ECS state.
	/// - Main Functionality: Uses reflection to scan assemblies for all types implementing <see cref="IComponent"/> to build a mapping of component IDs to their names.
	/// - Usefulness: Enables debug tools (like <see cref="ArchetypeInspector"/> and <see cref="EcsTimeline2"/>) to display human-readable component names instead of raw IDs.
	/// </summary>
	public static class DebugUtils
	{
		private static SparseSet<string> _cachedComponentNames;

		public static SparseSet<string> BuildComponentSparseSet()
		{
			if (_cachedComponentNames != null)
			{
				return _cachedComponentNames;
			}

			var componentsData = AppDomain.CurrentDomain
				.GetAssemblies()
				.Where(a => !a.IsDynamic) // skip dynamic assemblies
				.SelectMany(a =>
				{
					try
					{
						return a.GetTypes();
					}
					catch (ReflectionTypeLoadException e)
					{
						return e.Types.Where(t => t != null);
					}
				})
				.Where(t => typeof(IComponent).IsAssignableFrom(t) &&
					!t.IsAbstract)
				// Makes Tuples (Id, Name)
				.Select(t =>
					{
						var id = (Activator.CreateInstance(t) as IComponent)!.TypeId;
						var name = t.ToString().Split('.')[^1];
						return (id, name);
					}
				)
				.ToArray();

			var componentNames = new SparseSet<string>(componentsData.Length);
			foreach (var tuple in componentsData)
			{
				componentNames.Set(tuple.id, tuple.name);
			}

			_cachedComponentNames = componentNames;
			return componentNames;
		}
	}
}
#endif