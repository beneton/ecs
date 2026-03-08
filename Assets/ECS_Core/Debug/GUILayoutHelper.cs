#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Beneton.ECS.Core.Editor
{
	/// <summary>
	/// Provides static helper methods to simplify and automate common <see cref="GUILayout"/> operations in the Unity Editor.
	/// - Intention: To simplify the calculation of GUI element heights (e.g., for multi-line labels) and provide consistent styling.
	/// - Main Functionality: Wraps standard Unity GUILayout methods, automatically adjusting the layout to accommodate content based on available view width.
	/// </summary>
	public static class GUILayoutHelper
	{
		private static float CalculateHeight(string content, GUIStyle style)
		{
			return style.CalcHeight(new GUIContent(content), EditorGUIUtility.currentViewWidth);
		}

		public static void SelectableLabel(string content, GUIStyle style)
		{
			var height = CalculateHeight(content, style);
			EditorGUILayout.SelectableLabel(content, style, GUILayout.Height(height));
		}

		public static void Label(string content, GUIStyle style)
		{
			var height = CalculateHeight(content, style);
			EditorGUILayout.LabelField(content, style, GUILayout.Height(height));
		}

		public static bool Button(string content, GUIStyle style)
		{
			var height = CalculateHeight(content, style);
			return GUILayout.Button(content, style, GUILayout.Height(height));
		}
	}
}
#endif