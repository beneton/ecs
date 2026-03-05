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
			_isActive = GUILayout.Toggle(_isActive, "Active");

			if (!Application.isPlaying)
			{
				EditorGUILayout.LabelField("Only works in play mode");
				_componentNames = null;
				_ecsDebugRef = null;
				return;
			}

			if (!_isActive)
			{
				EditorGUILayout.LabelField("Not Active");
				return;
			}

			_componentNames ??= DebugUtils.BuildComponentSparseSet();
			_ecsDebugRef ??= FindFirstObjectByType<ECSDebugRef>();

			var world = _ecsDebugRef.World;
			var componentManager = _ecsDebugRef.ComponentManager;

			var allArchetypes = componentManager.GetAllArchetypes();

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
						$"{archetype.Id.ToString()} (Entity Count: {entitiesInArchetype.Length})",
						EditorStyles.boldLabel);
					EditorGUILayout.LabelField(" Required: " + string.Join(',', requiredNames));
					EditorGUILayout.LabelField(" Excluded: " + string.Join(',', excludedNames));

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
			GUILayout.EndScrollView();
		}
	}
}
#endif