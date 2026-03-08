#if UNITY_EDITOR
using UnityEngine;

namespace Beneton.ECS.Core.Editor
{
	/// <summary>
	/// A helper <see cref="MonoBehaviour"/> that holds references to the <see cref="World"/> and <see cref="ComponentManager"/>.
	/// - Purpose: Allows Editor-only debug windows (like <see cref="EntityInspector"/> or <see cref="EcsTimeline"/>) to access the active ECS state.
	/// - Lifecycle: Automatically created by the <see cref="World"/> constructor when running in the Unity Editor.
	/// </summary>
	public class ECSDebugRef : MonoBehaviour
	{
		internal World World;
		internal ComponentManager ComponentManager;
	}
}
#endif