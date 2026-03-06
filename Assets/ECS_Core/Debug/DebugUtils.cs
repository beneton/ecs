#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;

namespace Beneton.ECS.Core.Editor
{
	public static class DebugUtils
	{
		public static SparseSet<string> BuildComponentSparseSet()
		{
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

			return componentNames;
		}
	}
}
#endif