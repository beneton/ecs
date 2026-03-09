#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Beneton.ECS.Core.Editor
{
	/// <summary>
	/// Provides a real-time debug interface for inspecting ECS entities and their components within the Unity Editor.
	/// - Intention: To bridge the gap between the Unity hierarchy and the internal ECS state during Play Mode.
	/// - Usefulness: Facilitates debugging by allowing developers to select a <see cref="GameObject"/> and view the full data of its associated <see cref="Entity"/>,
	/// including all attached components and their current values formatted as JSON.
	/// </summary>
	public class EntityInspector : EditorWindow
	{
		[Serializable]
		private class FormattedComponentData
		{
			public string Name;
			public string Id;
			public IComponent Component;
		}

		private class ComponentUI
		{
			public Foldout Foldout;
			public SerializedObject SerializedObject;
			public ComponentWrapper Wrapper;
			public IComponent LastData;
		}

		private class ComponentWrapper : ScriptableObject
		{
			[SerializeReference]
			public IComponent Component;
		}

		private EcsDebugRef _ecsDebugRef;
		private bool _isActive = true;
		private bool _orderByName = false;
		private string _searchFilter = string.Empty;

		private ToolbarToggle _activeToggle;
		private ToolbarToggle _orderToggle;
		private ToolbarSearchField _searchField;
		private ToolbarMenu _selectEntityMenu;
		private IntegerField _selectEntityField;
		private Label _statusLabel;
		private ScrollView _scrollView;

		private GameObject _lastSelectedGo;
		private Entity _lastEntity;
		private readonly Dictionary<int, ComponentUI> _componentCache = new();
		private List<int> _currentVisibleComponentIds = new();

		[MenuItem("Ecs Debug/Entity Inspector")]
		public static void ShowWindow()
		{
			GetWindow<EntityInspector>("Entity Inspector");
		}

		private void OnEnable()
		{
			Selection.selectionChanged += UpdateUI;
		}

		private void OnDisable()
		{
			Selection.selectionChanged -= UpdateUI;
		}

		public void CreateGUI()
		{
			// Toolbar
			var toolbar = new Toolbar { style = { flexShrink = 0 } };

			_activeToggle = new ToolbarToggle { text = "Active", value = _isActive };
			_activeToggle.RegisterValueChangedCallback(evt => { _isActive = evt.newValue; });
			toolbar.Add(_activeToggle);

			_orderToggle = new ToolbarToggle { text = "Order By Name", value = _orderByName };
			_orderToggle.RegisterValueChangedCallback(evt =>
			{
				_orderByName = evt.newValue;
				FullRebuild();
			});
			toolbar.Add(_orderToggle);

			_searchField = new ToolbarSearchField
			{
				style =
				{
					flexGrow = 1,
					minWidth = 100,
					flexShrink = 1
				}
			};
			_searchField.RegisterValueChangedCallback(evt =>
			{
				_searchFilter = evt.newValue;
				ApplyFiltering();
			});
			toolbar.Add(_searchField);

			rootVisualElement.Add(toolbar);


			_selectEntityField = new IntegerField("Select Entity By Id")
			{
				style = { minWidth = 100, marginLeft = 5 }
			};
			_selectEntityField.labelElement.style.minWidth = 80;
			_selectEntityField.RegisterValueChangedCallback(evt => { SelectEntity(evt.newValue); });
			rootVisualElement.Add(_selectEntityField);

			// Status Label
			_statusLabel = new Label
			{
				style =
				{
					paddingTop = 10,
					paddingLeft = 5,
					unityFontStyleAndWeight = FontStyle.Bold
				}
			};
			rootVisualElement.Add(_statusLabel);

			// ScrollView
			_scrollView = new ScrollView
			{
				style =
				{
					flexGrow = 1
				}
			};
			rootVisualElement.Add(_scrollView);

			// Schedule update
			rootVisualElement.schedule.Execute(UpdateUI).Every(100);
		}

		private void FullRebuild()
		{
			_lastSelectedGo = null;
			_lastEntity = Entity.Null;
			_componentCache.Clear();
			UpdateUI();
		}

		private void ApplyFiltering()
		{
			foreach (var kvp in _componentCache)
			{
				var componentName = kvp.Value.Foldout.text;
				var visible = string.IsNullOrEmpty(_searchFilter) || componentName.IndexOf(
					_searchFilter,
					StringComparison.OrdinalIgnoreCase) >= 0;
				kvp.Value.Foldout.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
			}
		}

		private void SelectEntity(int entityId)
		{
			if (!Application.isPlaying || entityId <= 0)
			{
				return;
			}

			var entity = new Entity(entityId);
			if (_ecsDebugRef.World.TryGetGameObject(entity, out var go))
			{
				Selection.activeGameObject = go;
				EditorGUIUtility.PingObject(go);
			}
		}

		private void UpdateUI()
		{
			if (!Application.isPlaying)
			{
				_statusLabel.text = "Only works in play mode";
				_statusLabel.style.display = DisplayStyle.Flex;
				_scrollView.Clear();
				_ecsDebugRef = null;
				_lastSelectedGo = null;
				_componentCache.Clear();
				return;
			}

			if (!_isActive)
			{
				return;
			}

			if (Selection.activeGameObject == null)
			{
				_statusLabel.text = "Select a Game Object in Hierarchy";
				_statusLabel.style.display = DisplayStyle.Flex;
				_scrollView.Clear();
				_lastSelectedGo = null;
				_componentCache.Clear();
				return;
			}

			_ecsDebugRef ??= FindFirstObjectByType<EcsDebugRef>();
			if (_ecsDebugRef == null)
			{
				_statusLabel.text = "Waiting for ECS initialization...";
				_statusLabel.style.display = DisplayStyle.Flex;
				return;
			}

			var world = _ecsDebugRef.World;
			var componentManager = _ecsDebugRef.ComponentManager;
			var selectedGo = Selection.activeGameObject;

			if (!world.TryGetEntity(selectedGo, out var entity))
			{
				_statusLabel.text = "Selected Game Object has no Entity associated with it";
				_statusLabel.style.display = DisplayStyle.Flex;
				_scrollView.Clear();
				_lastSelectedGo = null;
				_componentCache.Clear();
				return;
			}

			_statusLabel.style.display = DisplayStyle.None;

			var rawComponents = componentManager.GetAllComponents(entity);
			var componentNames = DebugUtils.BuildComponentSparseSet();

			var allComponents = rawComponents.Select(c =>
			{
				var compName = componentNames.Has(c.TypeId)
					? componentNames.Get(c.TypeId)
					: c.GetType().ToString().Split('.')[^1];
				return new FormattedComponentData
				{
					Name = compName,
					Id = c.TypeId.ToString(),
					Component = c
				};
			}).ToList();

			if (_orderByName)
			{
				allComponents = allComponents.OrderBy(c => c.Name).ToList();
			}

			var currentIds = allComponents.Select(c => int.Parse(c.Id)).ToList();

			// If entity or component set changed, full rebuild of scrollview content
			if (selectedGo != _lastSelectedGo ||
				!entity.Equals(_lastEntity) ||
				!currentIds.SequenceEqual(_currentVisibleComponentIds))
			{
				_scrollView.Clear();
				_componentCache.Clear();
				_lastSelectedGo = selectedGo;
				_lastEntity = entity;
				_currentVisibleComponentIds = currentIds;

				// Entity Header
				var headerContainer = new VisualElement
				{
					style =
					{
						flexDirection = FlexDirection.Row, alignItems = Align.Center,
						paddingBottom = 5, paddingTop = 5, paddingLeft = 5
					}
				};
				var entityName = $"Entity: {selectedGo.name}";
				var entityLabel = new Label(entityName)
				{
					style =
					{
						unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1
					}
				};
				headerContainer.Add(entityLabel);

				var timelineButton = new Button(() =>
				{
					var timeline = GetWindow<EcsTimeline>("Ecs Timeline");
					timeline.SetEntityFilter(entity.Id.ToString());
					timeline.Focus();
				})
				{
					text = "Filter Timeline",
					style = { height = 20, fontSize = 10 }
				};
				headerContainer.Add(timelineButton);

				_scrollView.Add(headerContainer);

				foreach (var component in allComponents)
				{
					var id = int.Parse(component.Id);
					var foldout = new Foldout
						{ text = $"{component.Name} ({component.Id})", value = true };

					// Copy button
					var copyBtn = new Button(() =>
					{
						var json = EditorJsonUtility.ToJson(component.Component, true);
						GUIUtility.systemCopyBuffer = $"{component.Name}\n{json}";
						Debug.Log($"Copied {component.Name} data to clipboard");
					})
					{
						text = "Copy Data",
						style =
						{
							position = Position.Absolute, right = 2, height = 18,
							fontSize = 10, marginTop = 0,
							width = 75
						}
					};
					// Find the toggle (header) of the foldout to add the button there
					var toggle = foldout.Q<Toggle>();
					if (toggle != null)
					{
						toggle.Add(copyBtn);
					}
					else
					{
						foldout.Add(copyBtn);
					}

					var wrapper = CreateInstance<ComponentWrapper>();
					wrapper.Component = component.Component;
					var so = new SerializedObject(wrapper);
					var prop = so.FindProperty("Component");

					var propField = new PropertyField(prop);
					propField.Bind(so);
					propField.SetEnabled(false);

					// Ensure that the PropertyField is expanded to show its children
					// Since it's a [SerializeReference] IComponent, it might be collapsed by default
					propField.RegisterCallback<AttachToPanelEvent, PropertyField>(
						(ev, propertyField) =>
						{
							var toggle = propertyField.Q<Toggle>();
							if (toggle != null)
							{
								toggle.value = true;
							}
						},
						propField);

					foldout.Add(propField);
					_scrollView.Add(foldout);

					_componentCache[id] = new ComponentUI
					{
						Foldout = foldout,
						SerializedObject = so,
						Wrapper = wrapper,
						LastData = component.Component
					};
				}

				ApplyFiltering();
			}
			else
			{
				// Update values only if changed
				foreach (var component in allComponents)
				{
					var id = int.Parse(component.Id);
					if (_componentCache.TryGetValue(id, out var ui))
					{
						if (!ui.LastData.Equals(component.Component))
						{
							ui.Wrapper.Component = component.Component;
							ui.SerializedObject.Update();
							ui.LastData = component.Component;
						}
					}
				}
			}
		}
	}
}
#endif