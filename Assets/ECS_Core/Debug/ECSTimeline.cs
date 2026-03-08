#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Beneton.ECS.Core.Editor
{
	/// <summary>
	/// Provides a real-time, chronological log of component lifecycle events within the ECS for debugging purposes.
	/// - Intention: To capture and display a sequence of "Adding", "Updating", and "Removing" component operations in the Unity Editor during Play Mode.
	/// - Usefulness: Helps developers trace the exact order and timing of state changes for entities, making it easier to identify unexpected behaviors or race conditions in system logic.
	/// </summary>
	public class EcsTimeline : EditorWindow, ITimelineHandler
	{
		// Key is Component.Id
		private SparseSet<string> _componentNames;

		private SparseSet<string> ComponentNames
		{
			get
			{
				if (_componentNames == null)
				{
					_componentNames = DebugUtils.BuildComponentSparseSet();
				}

				return _componentNames;
			}
		}

		private Vector2 _scrollPosition = Vector2.zero;
		private Rect _scrollViewRect;
		private Rect _lastContentRect;
		private bool _isActive = true;

		// Filtering
		private string _entityFilter = string.Empty;
		private string _componentFilter = string.Empty;
		private bool _showAdd = true;
		private bool _showUpdate = true;
		private bool _showRemove = true;

		private const int MaxEvents = 1000;
		private readonly List<TimelineEvent> _eventEntries = new();
		private bool _autoScroll = true;

		private ECSDebugRef _ecsDebugRef;

		private ECSDebugRef ECSDebugRef
		{
			get
			{
				if (_ecsDebugRef == null)
				{
					_ecsDebugRef = FindFirstObjectByType<ECSDebugRef>();
				}

				return _ecsDebugRef;
			}
		}

		[MenuItem("Debug/ECS Timeline")]
		public static void ShowWindow()
		{
			GetWindow<EcsTimeline>("ECS Timeline");
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
			DrawToolbar();

			if (!Application.isPlaying)
			{
				EditorGUILayout.LabelField("Only works in play mode");
				_eventEntries.Clear();
				_lastContentRect = new Rect();
				_scrollViewRect = new Rect();
				_ecsDebugRef = null;
				_componentNames = null;
				return;
			}

			DrawFilters();

			var richTextStyle = new GUIStyle(EditorStyles.label)
			{
				richText = true
			};

			if (ECSDebugRef != null && ECSDebugRef.ComponentManager != null)
			{
				ECSDebugRef.ComponentManager.SetTimelineHandler(this);
			}

			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
			{
				foreach (var eventEntry in _eventEntries)
				{
					if (!PassesFilter(eventEntry))
					{
						continue;
					}

					EditorGUILayout.BeginHorizontal();
					{
						var timestamp = $"[{eventEntry.FrameCount}] ({eventEntry.Realtime:F2}s)";
						GUILayout.Label(timestamp, GUILayout.Width(120));

						if (GUILayout.Button(eventEntry.FormattedMessage, richTextStyle))
						{
							OnEventClicked(eventEntry);
						}

						if (GUILayout.Button("Inspect", GUILayout.Width(60)))
						{
							InspectEntity(eventEntry.EntityId);
						}
					}
					EditorGUILayout.EndHorizontal();

					_lastContentRect = GUILayoutUtility.GetLastRect();
				}

				if (_autoScroll && Event.current.type == EventType.Repaint)
				{
					var viewportSize = _scrollViewRect.height;
					var contentSize = _lastContentRect.y + _lastContentRect.height;
					if (contentSize > viewportSize)
					{
						_scrollPosition.y = contentSize;
					}
				}
			}
			GUILayout.EndScrollView();
			_scrollViewRect = GUILayoutUtility.GetLastRect();
		}

		private void DrawToolbar()
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			{
				_isActive = GUILayout.Toggle(_isActive, "Active", EditorStyles.toolbarButton);
				_autoScroll = GUILayout.Toggle(
					_autoScroll,
					"Auto-Scroll",
					EditorStyles.toolbarButton);

				if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
				{
					_eventEntries.Clear();
				}

				if (GUILayout.Button("Export", EditorStyles.toolbarButton))
				{
					ExportLogs();
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DrawFilters()
		{
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			{
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField("Filters:", GUILayout.Width(50));
					_entityFilter = EditorGUILayout.TextField("Entity Name/ID", _entityFilter);
					_componentFilter = EditorGUILayout.TextField("Component", _componentFilter);
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Space(55);
					_showAdd = GUILayout.Toggle(_showAdd, "Adding", GUILayout.Width(70));
					_showUpdate = GUILayout.Toggle(_showUpdate, "Updating", GUILayout.Width(80));
					_showRemove = GUILayout.Toggle(_showRemove, "Removing", GUILayout.Width(80));
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();
		}

		private bool PassesFilter(TimelineEvent ev)
		{
			if (!_showAdd && ev.Type == TimelineEventType.AddComponent)
			{
				return false;
			}

			if (!_showUpdate && ev.Type == TimelineEventType.UpdateComponent)
			{
				return false;
			}

			if (!_showRemove && (ev.Type == TimelineEventType.RemoveComponent ||
				ev.Type == TimelineEventType.RemoveAllComponents))
			{
				return false;
			}

			if (!string.IsNullOrEmpty(_entityFilter))
			{
				if (!ev.EntityName.Contains(_entityFilter) &&
					!ev.EntityId.ToString().Contains(_entityFilter))
				{
					return false;
				}
			}

			if (!string.IsNullOrEmpty(_componentFilter))
			{
				if (ev.ComponentName == null || !ev.ComponentName.Contains(_componentFilter))
				{
					return false;
				}
			}

			return true;
		}

		private void OnEventClicked(TimelineEvent ev)
		{
			var world = ECSDebugRef.World;
			if (world.TryGetGameObject(new Entity(ev.EntityId), out var gameObject))
			{
				EditorGUIUtility.PingObject(gameObject);
				Selection.activeGameObject = gameObject;
			}
		}

		private void InspectEntity(int entityId)
		{
			var world = ECSDebugRef.World;
			if (world.TryGetGameObject(new Entity(entityId), out var gameObject))
			{
				Selection.activeGameObject = gameObject;
				EntityInspector.ShowWindow();
			}
		}

		private void ExportLogs()
		{
			var path = EditorUtility.SaveFilePanel(
				"Export ECS Timeline",
				"",
				"ecs_timeline.txt",
				"txt");
			if (string.IsNullOrEmpty(path))
			{
				return;
			}

			var lines = _eventEntries.Select(e =>
				$"[{e.FrameCount}] ({e.Realtime:F2}s) {e.Type}: Entity={e.EntityName}({e.EntityId}), Component={e.ComponentName}");
			File.WriteAllLines(path, lines);
			Debug.Log($"ECS Timeline exported to {path}");
		}

		public void RegisterAddComponent(Entity entity, string entityName, int componentId)
		{
			if (!_isActive)
			{
				return;
			}

			var componentName = ComponentNames.Get(componentId);
			RegisterEvent(
				new TimelineEvent
				{
					EntityId = entity.Id,
					EntityName = entityName,
					ComponentId = componentId,
					ComponentName = componentName,
					Type = TimelineEventType.AddComponent,
					FrameCount = Time.frameCount,
					Realtime = Time.realtimeSinceStartup,
					FormattedMessage =
						$"<color=#5CFF5B>Adding <b>{componentName}</b> to <b>{entityName}</b></color>"
				});
		}

		public void RegisterUpdateComponent(Entity entity, string entityName, int componentId)
		{
			if (!_isActive)
			{
				return;
			}

			var componentName = ComponentNames.Get(componentId);
			RegisterEvent(
				new TimelineEvent
				{
					EntityId = entity.Id,
					EntityName = entityName,
					ComponentId = componentId,
					ComponentName = componentName,
					Type = TimelineEventType.UpdateComponent,
					FrameCount = Time.frameCount,
					Realtime = Time.realtimeSinceStartup,
					FormattedMessage =
						$"<color=#FFFF5A>Updating <b>{componentName}</b> in <b>{entityName}</b></color>"
				});
		}

		public void RegisterRemoveComponent(Entity entity, string entityName, int componentId)
		{
			if (!_isActive)
			{
				return;
			}

			var componentName = ComponentNames.Get(componentId);
			RegisterEvent(
				new TimelineEvent
				{
					EntityId = entity.Id,
					EntityName = entityName,
					ComponentId = componentId,
					ComponentName = componentName,
					Type = TimelineEventType.RemoveComponent,
					FrameCount = Time.frameCount,
					Realtime = Time.realtimeSinceStartup,
					FormattedMessage =
						$"<color=#FF5A6C>Removing <b>{componentName}</b> from <b>{entityName}</b></color>"
				});
		}

		public void RegisterRemoveAllComponent(Entity entity, string entityName)
		{
			if (!_isActive)
			{
				return;
			}

			RegisterEvent(
				new TimelineEvent
				{
					EntityId = entity.Id,
					EntityName = entityName,
					ComponentId = -1,
					ComponentName = "All",
					Type = TimelineEventType.RemoveAllComponents,
					FrameCount = Time.frameCount,
					Realtime = Time.realtimeSinceStartup,
					FormattedMessage =
						$"<color=#FF5A6C>Removing <b>all components</b> from <b>{entityName}</b></color>"
				});
		}

		private void RegisterEvent(TimelineEvent ev)
		{
			if (_eventEntries.Count >= MaxEvents)
			{
				_eventEntries.RemoveAt(0);
			}

			_eventEntries.Add(ev);
		}
	}
}
#endif