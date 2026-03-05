using System.Linq;

namespace Beneton.ECS.Core
{
	public class Archetype
	{
		private readonly int _id;

		private readonly int[] _required;
		private readonly int[] _exclude;

		public int Id => _id;

		public Archetype(int id, int[] required, int[] exclude)
		{
			_id = id;
			_required = required;
			_exclude = exclude;
		}

		public bool Equals(int[] required, int[] exclude)
		{
			if (_required.Length != required.Length || _exclude.Length != exclude.Length)
			{
				return false;
			}

			foreach (var candidate in required)
			{
				if (!_required.Contains(candidate))
				{
					return false;
				}
			}

			foreach (var candidate in exclude)
			{
				if (!_exclude.Contains(candidate))
				{
					return false;
				}
			}

			return true;
		}

		public bool Matches(Entity entity, ComponentManager componentManager)
		{
			foreach (var component in _required)
			{
				if (!componentManager.HasComponentFast(entity, component))
				{
					return false;
				}
			}

			foreach (var component in _exclude)
			{
				if (componentManager.HasComponentFast(entity, component))
				{
					return false;
				}
			}

			return true;
		}
		
		#if UNITY_EDITOR
		public (int[] Required, int[] Excluded) GetComponents()
		{
			return (_required, _exclude);
		}
		#endif
	}
}