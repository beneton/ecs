#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Beneton.ECS.Core.Editor
{
	public class ArchetypeInspector : EditorWindow
	{
		private Vector2 _scrollPosition = Vector2.zero;
		private ECSDebugRef _ecsDebugRef;
		private bool _isActive = true;

		// Key is Component.Id
		private SparseSet<string> _componentNames;

		private SparseSet<bool> _foldouts;

		[MenuItem("Debug/Archetype Inspector")]
		public static void ShowWindow()
		{
			GetWindow<ArchetypeInspector>("Archetype Inspector");
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
			EditorGUILayout.BeginHorizontal();
			{
				_isActive = GUILayout.Toggle(_isActive, "Active");
				if (GUILayout.Button("Collapse All"))
				{
					foreach (var foldoutsKey in _foldouts.Keys)
					{
						_foldouts.Set(foldoutsKey, false);
					}
				}

				if (GUILayout.Button("Expand All"))
				{
					foreach (var foldoutsKey in _foldouts.Keys)
					{
						_foldouts.Set(foldoutsKey, true);
					}
				}
			}
			EditorGUILayout.EndHorizontal();

			if (!Application.isPlaying)
			{
				EditorGUILayout.LabelField("Only works in play mode");
				_componentNames = null;
				_ecsDebugRef = null;
				_foldouts = null;
				return;
			}

			if (!_isActive)
			{
				EditorGUILayout.LabelField("Not Active");
				return;
			}

			var richTextStyle = new GUIStyle(EditorStyles.label)
			{
				richText = true
			};

			_componentNames ??= DebugUtils.BuildComponentSparseSet();
			_ecsDebugRef ??= FindFirstObjectByType<ECSDebugRef>();

			var world = _ecsDebugRef.World;
			var componentManager = _ecsDebugRef.ComponentManager;

			var allArchetypes = componentManager.GetAllArchetypes();
			_foldouts ??= new SparseSet<bool>();

			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
			{
				foreach (var archetype in allArchetypes)
				{
					var entitiesInArchetype = componentManager.GetEntities(archetype);
					var archetypeComponents = archetype.GetComponents();
					var requiredNames = archetypeComponents.Required
						.Select(c => _componentNames.Get(c));
					var excludedNames = archetypeComponents.Excluded
						.Select(c => _componentNames.Get(c));

					EditorGUILayout.LabelField(
						$"<b>Archetype {archetype.Id.ToString()}</b>",
						richTextStyle);
					EditorGUILayout.LabelField(
						" Required: <color=#5CFF5B>" + string.Join(", ", requiredNames) +
						"</color>",
						richTextStyle);
					EditorGUILayout.LabelField(
						" Excluded: <color=#FF5A6C>" + string.Join(", ", excludedNames) +
						"</color>",
						richTextStyle);

					var foldout = _foldouts.TryGet(archetype.Id, out var exists);
					if (!exists)
					{
						foldout = true;
					}

					foldout = EditorGUILayout.BeginFoldoutHeaderGroup(
						foldout,
						$"Entity list (Count: {entitiesInArchetype.Length})");
					{
						_foldouts.Set(archetype.Id, foldout);
						if (foldout)
						{
							foreach (var entity in entitiesInArchetype)
							{
								if (world.TryGetGameObject(entity, out var go))
								{
									if (GUILayoutHelper.Button(go.name, EditorStyles.miniButton))
									{
										Selection.activeGameObject = go;
									}
								}
								else
								{
									GUILayoutHelper.Label(entity.ToString(), EditorStyles.label);
								}
							}
						}
					}
					EditorGUILayout.EndFoldoutHeaderGroup();
					EditorGUILayout.Separator();
				}
			}
			GUILayout.EndScrollView();
		}
	}
}
#endif