#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Beneton.ECS.Core.Editor
{
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