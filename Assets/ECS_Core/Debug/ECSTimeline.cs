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
		private bool _autoSelectInHierarchy = true;

		// Filtering
		private string _entityFilter = string.Empty;
		private string _entityExcludeFilter = string.Empty;
		private string _componentFilter = string.Empty;
		private string _componentExcludeFilter = string.Empty;
		private string _systemFilter = string.Empty;
		private string _systemExcludeFilter = string.Empty;
		private bool _showAdd = true;
		private bool _showUpdate = true;
		private bool _showRemove = true;

		private const int MaxEvents = 100000;
		private readonly List<TimelineEvent> _eventEntries = new();
		private readonly List<TimelineEvent> _filteredEntries = new();
		
		private static readonly char[] FilterChars = new[] { ',', ';' };
		
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

			var autoSelectToggle = new ToolbarToggle
			{
				text = "Auto-Select", value = _autoSelectInHierarchy,
				tooltip = "Auto-select GameObject in Hierarchy when a line is selected"
			};
			autoSelectToggle.RegisterValueChangedCallback(evt =>
				_autoSelectInHierarchy = evt.newValue);
			toolbar.Add(autoSelectToggle);

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

			// Filter Table Layout
			const float labelWidth = 80f;

			// Header Row
			var headerRow = new VisualElement
			{
				style =
				{
					flexDirection = FlexDirection.Row, marginBottom = 4, borderBottomWidth = 1,
					borderBottomColor = Color.gray
				}
			};
			headerRow.Add(
				new Label("Filter Type")
					{ style = { width = labelWidth, unityFontStyleAndWeight = FontStyle.Bold } });
			headerRow.Add(
				new Label("Show (Inclusion)")
				{
					style =
					{
						flexGrow = 1, flexBasis = 0, unityFontStyleAndWeight = FontStyle.Bold,
						marginLeft = 4
					}
				});
			headerRow.Add(
				new Label("Hide (Exclusion)")
				{
					style =
					{
						flexGrow = 1, flexBasis = 0, unityFontStyleAndWeight = FontStyle.Bold,
						marginLeft = 10
					}
				});
			_filterSection.Add(headerRow);

			// Entity Row
			var entityRow = new VisualElement
				{ style = { flexDirection = FlexDirection.Row, marginBottom = 2 } };
			entityRow.Add(
				new Label("Entity")
					{ style = { width = labelWidth, unityTextAlign = TextAnchor.MiddleLeft } });

			var entityField = new TextField
				{ value = _entityFilter, style = { flexGrow = 1, flexBasis = 0, marginLeft = 4 } };
			entityField.tooltip = "Filter by Entity Name/ID (comma/semicolon separated)";
			entityField.RegisterValueChangedCallback(evt => { _entityFilter = evt.newValue; });
			entityRow.Add(entityField);

			var entityExcludeField = new TextField
			{
				value = _entityExcludeFilter,
				style = { flexGrow = 1, flexBasis = 0, marginLeft = 10 }
			};
			entityExcludeField.tooltip = "Exclude by Entity Name/ID (comma/semicolon separated)";
			entityExcludeField.RegisterValueChangedCallback(evt =>
			{
				_entityExcludeFilter = evt.newValue;
			});
			entityRow.Add(entityExcludeField);

			_filterSection.Add(entityRow);

			// Component Row
			var componentRow = new VisualElement
				{ style = { flexDirection = FlexDirection.Row, marginBottom = 2 } };
			componentRow.Add(
				new Label("Component")
					{ style = { width = labelWidth, unityTextAlign = TextAnchor.MiddleLeft } });

			var componentField = new TextField
			{
				value = _componentFilter, style = { flexGrow = 1, flexBasis = 0, marginLeft = 4 }
			};
			componentField.tooltip = "Filter by component names (comma/semicolon separated)";
			componentField.RegisterValueChangedCallback(evt =>
			{
				_componentFilter = evt.newValue;
			});
			componentRow.Add(componentField);

			var componentExcludeField = new TextField
			{
				value = _componentExcludeFilter,
				style = { flexGrow = 1, flexBasis = 0, marginLeft = 10 }
			};
			componentExcludeField.tooltip = "Exclude component names (comma/semicolon separated)";
			componentExcludeField.RegisterValueChangedCallback(evt =>
			{
				_componentExcludeFilter = evt.newValue;
			});
			componentRow.Add(componentExcludeField);

			_filterSection.Add(componentRow);

			// System Row
			var systemRow = new VisualElement
				{ style = { flexDirection = FlexDirection.Row, marginBottom = 4 } };
			systemRow.Add(
				new Label("System")
				{
					style = { width = labelWidth, unityTextAlign = TextAnchor.MiddleLeft }
				});

			var systemField = new TextField
			{
				value = _systemFilter,
				style = { flexGrow = 1, flexBasis = 0, marginLeft = 4 }
			};
			systemField.tooltip = "Filter by system names (comma/semicolon separated)";
			systemField.RegisterValueChangedCallback(evt => { _systemFilter = evt.newValue; });
			systemRow.Add(systemField);

			var excludeField = new TextField
			{
				value = _systemExcludeFilter,
				style = { flexGrow = 1, flexBasis = 0, marginLeft = 10 }
			};
			excludeField.tooltip = "Exclude system names (comma/semicolon separated)";
			excludeField.RegisterValueChangedCallback(evt =>
			{
				_systemExcludeFilter = evt.newValue;
			});
			systemRow.Add(excludeField);

			_filterSection.Add(systemRow);

			// Row 4: Types & Reset
			var row4 = new VisualElement
			{
				style = { flexDirection = FlexDirection.Row, marginTop = 4 }
			};
			row4.Add(
				new Label("Event Types:")
				{
					style = { width = labelWidth, unityTextAlign = TextAnchor.MiddleLeft }
				});

			var addToggle = new ToolbarToggle
			{
				text = "Added", value = _showAdd, style = { flexGrow = 0 }
			};
			addToggle.RegisterValueChangedCallback(evt => { _showAdd = evt.newValue; });
			row4.Add(addToggle);

			var updToggle = new ToolbarToggle
			{
				text = "Updated", value = _showUpdate, style = { flexGrow = 0 }
			};
			updToggle.RegisterValueChangedCallback(evt => { _showUpdate = evt.newValue; });
			row4.Add(updToggle);

			var remToggle = new ToolbarToggle
			{
				text = "Removed", value = _showRemove, style = { flexGrow = 0 }
			};
			remToggle.RegisterValueChangedCallback(evt => { _showRemove = evt.newValue; });
			row4.Add(remToggle);

			row4.Add(new VisualElement { style = { flexGrow = 1 } });

			var resetButton = new Button(() =>
			{
				_entityFilter = string.Empty;
				_entityExcludeFilter = string.Empty;
				_componentFilter = string.Empty;
				_componentExcludeFilter = string.Empty;
				_systemFilter = string.Empty;
				_systemExcludeFilter = string.Empty;
				entityField.SetValueWithoutNotify(string.Empty);
				entityExcludeField.SetValueWithoutNotify(string.Empty);
				componentField.SetValueWithoutNotify(string.Empty);
				componentExcludeField.SetValueWithoutNotify(string.Empty);
				systemField.SetValueWithoutNotify(string.Empty);
				excludeField.SetValueWithoutNotify(string.Empty);
			})
			{
				text = "Reset Filters",
				style = { width = 100 }
			};

			row4.Add(resetButton);
			_filterSection.Add(row4);

			root.Add(_filterSection);

			// List View
			_listView = new MultiColumnListView
			{
				itemsSource = _filteredEntries,
				fixedItemHeight = 24,
				showAlternatingRowBackgrounds = AlternatingRowBackground.All,
				reorderable = false,
				showBoundCollectionSize = false,
				style = { flexGrow = 1 },
				selectionType = SelectionType.Single
			};

			_listView.selectionChanged += OnSelectionChanged;

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
					unityTextAlign = TextAnchor.UpperCenter,
					flexGrow = 1,
					fontSize = 20
				}
			};
			root.Add(_notPlayingLabel);

			RefreshVisibility();

			// Schedule update
			rootVisualElement.schedule.Execute(UpdateUI).Every(100);
		}

		private void OnSelectionChanged(IEnumerable<object> selection)
		{
			if (!_autoSelectInHierarchy || !Application.isPlaying || ECSDebugRef == null)
			{
				return;
			}

			if (selection.FirstOrDefault() is not TimelineEvent selectedEvent)
			{
				return;
			}

			var world = ECSDebugRef.World;
			if (world == null)
			{
				return;
			}

			var entity = new Entity(selectedEvent.EntityId);
			if (world.TryGetGameObject(entity, out var gameObject))
			{
				Selection.activeGameObject = gameObject;
				EditorGUIUtility.PingObject(gameObject);
			}
		}

		private void UpdateUI()
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

			if (_notPlayingLabel != null)
			{
				_notPlayingLabel.style.display = isPlaying ? DisplayStyle.None : DisplayStyle.Flex;
			}

			if (!isPlaying && _eventEntries.Count > 0)
			{
				_eventEntries.Clear();
				_filteredEntries.Clear();
				_listView?.Rebuild();
				UpdateFilteredList();
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

		private bool IsAtBottom()
		{
			var scrollView = _listView.Q<ScrollView>();
			var verticalScroller = scrollView.verticalScroller;

			// If the scroll view is not scrollable, we're at the bottom.
			if (verticalScroller.style.visibility == Visibility.Hidden)
			{
				return true;
			}

			// A small threshold to account for floating point errors or styling offsets
			const float threshold = 2.0f;
			return verticalScroller.value >= verticalScroller.highValue - threshold;
		}

		private void UpdateFilteredList()
		{
			var wasAtBottom = IsAtBottom();

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
				if (wasAtBottom)
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
				var filters = _entityFilter.Split(FilterChars);
				var match = false;
				foreach (var filter in filters)
				{
					var trimmed = filter.Trim();
					if (string.IsNullOrEmpty(trimmed))
					{
						continue;
					}

					if (ev.EntityName.Contains(trimmed) ||
						ev.EntityId.ToString().Contains(trimmed))
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

			if (!string.IsNullOrEmpty(_entityExcludeFilter))
			{
				var filters = _entityExcludeFilter.Split(FilterChars);
				foreach (var filter in filters)
				{
					var trimmed = filter.Trim();
					if (string.IsNullOrEmpty(trimmed))
					{
						continue;
					}

					if (ev.EntityName.Contains(trimmed) ||
						ev.EntityId.ToString().Contains(trimmed))
					{
						return false;
					}
				}
			}

			if (!string.IsNullOrEmpty(_componentFilter))
			{
				if (ev.ComponentName == null)
				{
					return false;
				}

				var filters = _componentFilter.Split(FilterChars);
				var match = false;
				foreach (var filter in filters)
				{
					var trimmed = filter.Trim();
					if (string.IsNullOrEmpty(trimmed))
					{
						continue;
					}

					if (ev.ComponentName.Contains(trimmed))
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

			if (!string.IsNullOrEmpty(_componentExcludeFilter))
			{
				if (ev.ComponentName != null)
				{
					var filters = _componentExcludeFilter.Split(FilterChars);
					foreach (var filter in filters)
					{
						var trimmed = filter.Trim();
						if (string.IsNullOrEmpty(trimmed))
						{
							continue;
						}

						if (ev.ComponentName.Contains(trimmed))
						{
							return false;
						}
					}
				}
			}

			if (!string.IsNullOrEmpty(_systemFilter))
			{
				if (ev.CallerName == null)
				{
					return false;
				}

				var filters = _systemFilter.Split(FilterChars);
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
					var filters = _systemExcludeFilter.Split(FilterChars);
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