#if UNITY_EDITOR
using UnityEngine;

namespace Beneton.ECS.Core.Editor
{
	public class ECSDebugRef : MonoBehaviour
	{
		internal World World;
		internal ComponentManager ComponentManager;
	}
}
#endif