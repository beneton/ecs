#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

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

		private bool _isActive = true;

		// Filtering
		private string _entityFilter = string.Empty;
		private string _componentFilter = string.Empty;
		private string _systemFilter = string.Empty;
		private string _systemExcludeFilter = string.Empty;
		private bool _showAdd = true;
		private bool _showUpdate = true;
		private bool _showRemove = true;

		private const int MaxEvents = 10000;
		private readonly List<TimelineEvent> _eventEntries = new();
		private readonly List<TimelineEvent> _filteredEntries = new();
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

		// UI Toolkit Elements
		private MultiColumnListView _listView;
		private VisualElement _filterSection;
		private VisualElement _notPlayingLabel;

		[MenuItem("Debug/ECS Timeline")]
		public static void ShowWindow()
		{
			GetWindow<EcsTimeline>("ECS Timeline");
		}

		public void CreateGUI()
		{
			var root = rootVisualElement;

			// Toolbar
			var toolbar = new Toolbar
			{
				style = { flexShrink = 0 }
			};

			var activeToggle = new ToolbarToggle { text = "Active", value = _isActive };
			activeToggle.RegisterValueChangedCallback(evt => _isActive = evt.newValue);
			toolbar.Add(activeToggle);

			var autoScrollToggle = new ToolbarToggle { text = "Auto-Scroll", value = _autoScroll };
			autoScrollToggle.RegisterValueChangedCallback(evt =>
			{
				_autoScroll = evt.newValue;
				if (_autoScroll)
				{
					ScrollToBottom();
				}
			});
			toolbar.Add(autoScrollToggle);

			var clearButton = new ToolbarButton(() => { _eventEntries.Clear(); })
				{ text = "Clear" };
			toolbar.Add(clearButton);

			var exportButton = new ToolbarButton(ExportLogs) { text = "Export" };
			toolbar.Add(exportButton);

			root.Add(toolbar);

			// Filters
			_filterSection = new VisualElement
			{
				style =
				{
					paddingLeft = 4,
					paddingRight = 4,
					paddingTop = 4,
					paddingBottom = 4,
					flexShrink = 0
				}
			};
			_filterSection.AddToClassList("help-box");

			// Row 1: Entity & Component
			var row1 = new VisualElement
				{ style = { flexDirection = FlexDirection.Row, marginBottom = 2 } };
			var entityField = new TextField("Entity")
				{ value = _entityFilter, style = { flexGrow = 1 } };
			entityField.labelElement.style.minWidth = 50;
			entityField.RegisterValueChangedCallback(evt => { _entityFilter = evt.newValue; });
			row1.Add(entityField);

			var componentField = new TextField("Component")
				{ value = _componentFilter, style = { flexGrow = 1, marginLeft = 10 } };
			componentField.labelElement.style.minWidth = 70;
			componentField.RegisterValueChangedCallback(evt =>
			{
				_componentFilter = evt.newValue;
			});
			row1.Add(componentField);
			_filterSection.Add(row1);

			// Row 2: System & Exclude
			var row2 = new VisualElement
				{ style = { flexDirection = FlexDirection.Row, marginBottom = 2 } };
			var systemField = new TextField("System")
				{ value = _systemFilter, style = { flexGrow = 1 } };
			systemField.labelElement.style.minWidth = 50;
			systemField.RegisterValueChangedCallback(evt => { _systemFilter = evt.newValue; });
			row2.Add(systemField);

			var excludeField = new TextField("Exclude")
				{ value = _systemExcludeFilter, style = { flexGrow = 1, marginLeft = 10 } };
			excludeField.labelElement.style.minWidth = 70;
			excludeField.RegisterValueChangedCallback(evt =>
			{
				_systemExcludeFilter = evt.newValue;
			});
			row2.Add(excludeField);
			_filterSection.Add(row2);

			// Row 3: Types & Reset
			var row3 = new VisualElement { style = { flexDirection = FlexDirection.Row } };
			row3.Add(
				new Label("Types:")
					{ style = { minWidth = 50, unityTextAlign = TextAnchor.MiddleLeft } });

			var addToggle = new ToolbarToggle { text = "Added", value = _showAdd };
			addToggle.RegisterValueChangedCallback(evt => { _showAdd = evt.newValue; });
			row3.Add(addToggle);

			var updToggle = new ToolbarToggle { text = "Updated", value = _showUpdate };
			updToggle.RegisterValueChangedCallback(evt => { _showUpdate = evt.newValue; });
			row3.Add(updToggle);

			var remToggle = new ToolbarToggle { text = "Removed", value = _showRemove };
			remToggle.RegisterValueChangedCallback(evt => { _showRemove = evt.newValue; });
			row3.Add(remToggle);

			row3.Add(new VisualElement { style = { flexGrow = 1 } });

			var resetButton = new Button(() =>
			{
				_entityFilter = string.Empty;
				_componentFilter = string.Empty;
				_systemFilter = string.Empty;
				_systemExcludeFilter = string.Empty;
				entityField.SetValueWithoutNotify(string.Empty);
				componentField.SetValueWithoutNotify(string.Empty);
				systemField.SetValueWithoutNotify(string.Empty);
				excludeField.SetValueWithoutNotify(string.Empty);
			})
			{
				text = "Reset Filters",
				style = { width = 100 }
			};

			row3.Add(resetButton);
			_filterSection.Add(row3);

			root.Add(_filterSection);

			// List View
			_listView = new MultiColumnListView
			{
				itemsSource = _filteredEntries,
				fixedItemHeight = 24,
				showAlternatingRowBackgrounds = AlternatingRowBackground.All,
				reorderable = false,
				showBoundCollectionSize = false,
				style = { flexGrow = 1 }
			};

			// Columns
			_listView.columns.Add(
				new Column
				{
					makeCell = () => new Label(),
					bindCell = (element, index) =>
					{
						var ev = _filteredEntries[index];
						(element as Label)!.text = ev.FormattedTiming;
					},
					title = "Frame (Time)",
					width = 110
				});

			_listView.columns.Add(
				new Column
				{
					makeCell = () => new Label
					{
						style =
						{
							unityTextAlign = TextAnchor.MiddleCenter,
							unityFontStyleAndWeight = FontStyle.Bold
						}
					},
					bindCell = (element, index) =>
					{
						var ev = _filteredEntries[index];
						var label = element as Label;
						label!.text = GetTypeShorthand(ev.Type);
						label.style.color = GetTypeColor(ev.Type);
					},
					title = "Type",
					width = 120
				});

			_listView.columns.Add(
				new Column
				{
					makeCell = () => new Label(),
					bindCell = (element, index) =>
					{
						var ev = _filteredEntries[index];
						(element as Label)!.text = ev.EntityName;
					},
					title = "Entity",
					width = 150
				});

			_listView.columns.Add(
				new Column
				{
					makeCell = () => new Label(),
					bindCell = (element, index) =>
					{
						var ev = _filteredEntries[index];
						(element as Label)!.text = ev.ComponentName;
					},
					title = "Component",
					width = 150
				});

			_listView.columns.Add(
				new Column
				{
					makeCell = () => new Label(),
					bindCell = (element, index) =>
					{
						var ev = _filteredEntries[index];
						(element as Label)!.text = ev.CallerName;
					},
					title = "Caller",
					width = 200,
					stretchable = true
				});

			root.Add(_listView);

			_notPlayingLabel = new Label("Only works in play mode")
			{
				style =
				{
					unityTextAlign = TextAnchor.MiddleCenter,
					flexGrow = 1,
					fontSize = 20
				}
			};
			root.Add(_notPlayingLabel);

			RefreshVisibility();
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
			RefreshVisibility();

			if (Application.isPlaying && ECSDebugRef != null)
			{
				UpdateFilteredList();
				if (ECSDebugRef.ComponentManager != null)
				{
					ECSDebugRef.ComponentManager.SetTimelineHandler(this);
				}
			}
		}

		private void RefreshVisibility()
		{
			var isPlaying = Application.isPlaying;
			if (_filterSection != null)
			{
				_filterSection.style.display = isPlaying ? DisplayStyle.Flex : DisplayStyle.None;
			}

			if (_listView != null)
			{
				_listView.style.display = isPlaying ? DisplayStyle.Flex : DisplayStyle.None;
			}

			if (_notPlayingLabel != null)
			{
				_notPlayingLabel.style.display = isPlaying ? DisplayStyle.None : DisplayStyle.Flex;
			}

			if (!isPlaying && _eventEntries.Count > 0)
			{
				_eventEntries.Clear();
				_filteredEntries.Clear();
				_listView?.Rebuild();
			}
		}

		private string GetTypeShorthand(TimelineEventType type)
		{
			return type switch
			{
				TimelineEventType.AddComponent => "Added",
				TimelineEventType.UpdateComponent => "Updated",
				TimelineEventType.RemoveComponent => "Removed",
				TimelineEventType.RemoveAllComponents => "Removed all",
				_ => type.ToString()
			};
		}

		private Color GetTypeColor(TimelineEventType type)
		{
			return type switch
			{
				TimelineEventType.AddComponent => new Color(0.36f, 1f, 0.36f),
				TimelineEventType.UpdateComponent => new Color(1f, 1f, 0.35f),
				TimelineEventType.RemoveComponent => new Color(1f, 0.35f, 0.42f),
				TimelineEventType.RemoveAllComponents => new Color(1f, 0.35f, 0.42f),
				_ => Color.white
			};
		}

		private void UpdateFilteredList()
		{
			_filteredEntries.Clear();
			foreach (var ev in _eventEntries)
			{
				if (PassesFilter(ev))
				{
					_filteredEntries.Add(ev);
				}
			}

			if (_listView != null)
			{
				_listView.Rebuild();
				if (_autoScroll)
				{
					ScrollToBottom();
				}
			}
		}

		private void ScrollToBottom()
		{
			if (_filteredEntries.Count > 0)
			{
				_listView.ScrollToItem(_filteredEntries.Count - 1);
			}
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

			if (!string.IsNullOrEmpty(_systemFilter))
			{
				if (ev.CallerName == null)
				{
					return false;
				}

				var filters = _systemFilter.Split(new[] { ',', ';' });
				var match = false;
				foreach (var filter in filters)
				{
					var trimmed = filter.Trim();
					if (string.IsNullOrEmpty(trimmed))
					{
						continue;
					}

					if (ev.CallerName.Contains(trimmed))
					{
						match = true;
						break;
					}
				}

				if (!match)
				{
					return false;
				}
			}

			if (!string.IsNullOrEmpty(_systemExcludeFilter))
			{
				if (ev.CallerName != null)
				{
					var filters = _systemExcludeFilter.Split(new[] { ',', ';' });
					foreach (var filter in filters)
					{
						var trimmed = filter.Trim();
						if (string.IsNullOrEmpty(trimmed))
						{
							continue;
						}

						if (ev.CallerName.Contains(trimmed))
						{
							return false;
						}
					}
				}
			}

			return true;
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
				$"[{e.FormattedTiming}) {e.Type}: Entity={e.EntityName}, Component={e.ComponentName}, System={e.CallerName}");
			File.WriteAllLines(path, lines);
			Debug.Log($"ECS Timeline exported to {path}");
		}

		public void RegisterAddComponent(
			Entity entity,
			string entityName,
			int componentId,
			string caller)
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
					CallerName = caller,
					Type = TimelineEventType.AddComponent,
					FormattedTiming = $"[{Time.frameCount}] ({Time.realtimeSinceStartup:F2}s)"
				});
		}

		public void RegisterUpdateComponent(
			Entity entity,
			string entityName,
			int componentId,
			string caller)
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
					CallerName = caller,
					Type = TimelineEventType.UpdateComponent,
					FormattedTiming = $"[{Time.frameCount}] ({Time.realtimeSinceStartup:F2}s)"
				});
		}

		public void RegisterRemoveComponent(
			Entity entity,
			string entityName,
			int componentId,
			string caller)
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
					CallerName = caller,
					Type = TimelineEventType.RemoveComponent,
					FormattedTiming = $"[{Time.frameCount}] ({Time.realtimeSinceStartup:F2}s)"
				});
		}

		public void RegisterRemoveAllComponent(Entity entity, string entityName, string caller)
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
					CallerName = caller,
					Type = TimelineEventType.RemoveAllComponents,
					FormattedTiming = $"[{Time.frameCount}] ({Time.realtimeSinceStartup:F2}s)"
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