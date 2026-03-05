#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Beneton.ECS.Core.Editor
{
	public class EntityInspector : EditorWindow
	{
		[Serializable]
		private class FormattedComponentData
		{
			public string Name;
			public string Id;
			public string Data;
		}

		private Vector2 _scrollPosition = Vector2.zero;
		private ECSDebugRef _ecsDebugRef;
		private bool _isActive = true;

		[MenuItem("Debug/Entity Inspector")]
		public static void ShowWindow()
		{
			GetWindow<EntityInspector>("Entity Inspector");
		}

		private void OnEnable()
		{
			EditorApplication.update += OnEditorUpdate;
		}

		private void OnDisable()
		{
			EditorApplication.update -= OnEditorUpdate;
		}

		private void OnEditorUpdate()
		{
			Repaint();
		}

		private void OnGUI()
		{
			_isActive = GUILayout.Toggle(_isActive, "Active");

			if (!Application.isPlaying)
			{
				EditorGUILayout.LabelField("Only works in play mode");
				_ecsDebugRef = null;
				return;
			}

			if (!_isActive)
			{
				EditorGUILayout.LabelField("Not Active");
				return;
			}

			if (Selection.activeGameObject == null)
			{
				EditorGUILayout.LabelField("Select a Game Object in Hierarchy");
				return;
			}

			_ecsDebugRef ??= FindFirstObjectByType<ECSDebugRef>();

			var world = _ecsDebugRef.World;
			var componentManager = _ecsDebugRef.ComponentManager;
			var selectedGo = Selection.activeGameObject;

			if (!world.TryGetEntity(selectedGo, out var entity))
			{
				EditorGUILayout.LabelField("Selected Game Object has no Entity associated with it");
				return;
			}

			var allComponents =
				componentManager
					.GetAllComponents(entity)
					.Select(c => new FormattedComponentData
					{
						Name = c.GetType().ToString().Split('.')[^1],
						Id = c.TypeId.ToString(),
						Data = EditorJsonUtility.ToJson(c, true)
					})
					.OrderBy(c => c.Name)
					.ToList();

			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
			{
				var entityName = $"{entity.ToString()} :: {selectedGo.name}";
				GUILayoutHelper.SelectableLabel(entityName, EditorStyles.boldLabel);

				foreach (var component in allComponents)
				{
					var componentName = $"{component.Name} ({component.Id})";
					GUILayoutHelper.SelectableLabel(componentName, EditorStyles.boldLabel);
					GUILayoutHelper.SelectableLabel(component.Data, EditorStyles.label);
				}
			}
			GUILayout.EndScrollView();
		}
	}
}
#endif