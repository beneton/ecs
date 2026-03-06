#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Beneton.ECS.Core.Editor
{
	public class ECSTimeline : EditorWindow, ITimelineHandler
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

		private readonly SparseSet<string> _eventEntries = new();
		private int _eventCount = 0;

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
			GetWindow<ECSTimeline>("ECS Timeline");
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
				_eventEntries.Clear();
				_eventCount = 0;
				_lastContentRect = new Rect();
				_scrollViewRect = new Rect();
				_ecsDebugRef = null;
				_componentNames = null;
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

			ECSDebugRef.ComponentManager.SetTimelineHandler(this);

			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
			{
				foreach (var eventEntry in _eventEntries.Values)
				{
					GUILayoutHelper.Label(eventEntry, richTextStyle);
					_lastContentRect = GUILayoutUtility.GetLastRect();
				}
			}
			GUILayout.EndScrollView();
			_scrollViewRect = GUILayoutUtility.GetLastRect();
		}

		public void RegisterAddComponent(Entity entity, int componentId)
		{
			if (!_isActive)
			{
				return;
			}

			var componentName = ComponentNames.Get(componentId);
			var world = ECSDebugRef.World;
			world.TryGetGameObject(entity, out var gameObject);

			RegisterEvent(
				$"<color=#5CFF5B>[{Time.frameCount}] Adding <b>{componentName}</b> to <b>{gameObject.name}</b></color>");
		}

		public void RegisterUpdateComponent(Entity entity, int componentId)
		{
			if (!_isActive)
			{
				return;
			}

			var componentName = ComponentNames.Get(componentId);
			var world = ECSDebugRef.World;
			world.TryGetGameObject(entity, out var gameObject);

			RegisterEvent(
				$"<color=#FFFF5A>[{Time.frameCount}] Updating <b>{componentName}</b> in <b>{gameObject.name}</b></color>");
		}


		public void RegisterRemoveComponent(Entity entity, int componentId)
		{
			if (!_isActive)
			{
				return;
			}

			var componentName = ComponentNames.Get(componentId);
			var world = ECSDebugRef.World;
			world.TryGetGameObject(entity, out var gameObject);

			RegisterEvent(
				$"<color=#FF5A6C>[{Time.frameCount}] Removing <b>{componentName}</b> from <b>{gameObject.name}</b></color>");
		}

		public void RegisterRemoveAllComponent(Entity entity)
		{
			if (!_isActive)
			{
				return;
			}

			var world = ECSDebugRef.World;
			world.TryGetGameObject(entity, out var gameObject);
			RegisterEvent(
				$"<color=#FF5A6C>[{Time.frameCount}] Removing <b>all components</b> from <b>{gameObject.name}</b></color>");
		}

		private void RegisterEvent(string eventString)
		{
			// Calculating Auto Scroll
			var viewportSize = _scrollViewRect.height - _scrollViewRect.y;
			var contentSize = _lastContentRect.y + _lastContentRect.height;

			if (contentSize - viewportSize <= _scrollPosition.y + 20)
			{
				_scrollPosition.y = float.MaxValue;
			}

			_eventEntries.Set(_eventCount, eventString);
			_eventCount++;
		}
	}
}
#endif