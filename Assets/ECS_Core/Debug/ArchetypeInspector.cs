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
	/// Provides a real-time debug interface for inspecting ECS archetypes and their entity memberships within the Unity Editor.
	/// - Intention: To provide a clear overview of how entities are grouped into archetypes based on their component composition.
	/// - Usefulness: Helps developers verify that entities are correctly matched to archetypes, inspect the required/excluded component sets for each archetype,
	/// and quickly locate specific entities in the Unity hierarchy for further inspection.
	/// </summary>
	public class ArchetypeInspector : EditorWindow
	{
		private EcsDebugRef2 _ecsDebugRef;
		private bool _isActive = true;
		private string _searchFilter = string.Empty;

		private ToolbarToggle _activeToggle;
		private ToolbarSearchField _searchField;
		private ScrollView _scrollView;
		private Label _statusLabel;
		private Label _statsLabel;

		private SparseSet<string> _componentNames;
		private readonly Dictionary<int, ArchetypeUI> _archetypeCache = new();
		private readonly List<Button> _buttonPool = new();

		private class ArchetypeUI
		{
			public VisualElement Root;
			public Label Title;
			public Foldout Foldout;
			public VisualElement RequiredContainer;
			public VisualElement ExcludedContainer;
			public VisualElement EntityList;
			public List<Button> EntityButtons = new();
			public List<Entity> Entities = new();
			public int LastEntityCount = -1;
		}

		[MenuItem("Debug/Archetype Inspector")]
		public static void ShowWindow()
		{
			GetWindow<ArchetypeInspector>("Archetype Inspector");
		}

		private void CreateGUI()
		{
			var root = rootVisualElement;

			// Toolbar
			var toolbar = new Toolbar { style = { flexShrink = 0 } };

			_activeToggle = new ToolbarToggle { text = "Active", value = _isActive };
			_activeToggle.RegisterValueChangedCallback(evt => { _isActive = evt.newValue; });
			toolbar.Add(_activeToggle);

			var expandAllButton = new ToolbarButton(() => SetAllFoldouts(true))
			{
				text = "Expand All",
				style = { flexShrink = 0 }
			};
			toolbar.Add(expandAllButton);

			var collapseAllButton = new ToolbarButton(() => SetAllFoldouts(false))
			{
				text = "Collapse All",
				style = { flexShrink = 0 }
			};
			toolbar.Add(collapseAllButton);

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

			root.Add(toolbar);

			// Stats Label
			_statsLabel = new Label
			{
				style =
				{
					paddingTop = 5,
					paddingLeft = 5,
					fontSize = 11,
					color = Color.gray,
					borderBottomWidth = 1,
					borderBottomColor = Color.gray,
					marginBottom = 5
				}
			};
			root.Add(_statsLabel);

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
			root.Add(_statusLabel);

			// ScrollView
			_scrollView = new ScrollView { style = { flexGrow = 1 } };
			root.Add(_scrollView);

			// Schedule update
			root.schedule.Execute(UpdateUI).Every(100);
		}

		private void OnDisable()
		{
			_archetypeCache.Clear();
			_buttonPool.Clear();
			_componentNames = null;
		}

		private void SetAllFoldouts(bool state)
		{
			foreach (var ui in _archetypeCache.Values)
			{
				ui.Foldout.value = state;
			}
		}

		private void UpdateUI()
		{
			if (!Application.isPlaying)
			{
				_scrollView.style.display = DisplayStyle.None;
				_statusLabel.style.display = DisplayStyle.Flex;
				_statusLabel.text = "Only works in play mode";
				_ecsDebugRef = null;
				return;
			}
			else
			{
				_scrollView.style.display = DisplayStyle.Flex;
				_statusLabel.style.display = DisplayStyle.None;
			}

			if (!_isActive)
			{
				return;
			}

			_ecsDebugRef ??= FindFirstObjectByType<EcsDebugRef2>();
			if (_ecsDebugRef == null)
			{
				_statusLabel.text = "Waiting for ECS initialization...";
				_statusLabel.style.display = DisplayStyle.Flex;
				return;
			}

			_componentNames ??= DebugUtils.BuildComponentSparseSet();

			var componentManager = _ecsDebugRef.ComponentManager;
			var world = _ecsDebugRef.World;
			var allArchetypes = componentManager.GetAllArchetypes();

			if (allArchetypes.Length == 0)
			{
				_statusLabel.text = "No archetypes found";
				_statusLabel.style.display = DisplayStyle.Flex;
				_scrollView.Clear();
				_statsLabel.text = string.Empty;
				return;
			}

			_statusLabel.style.display = DisplayStyle.None;

			var totalEntities = world.GetEntities().Length;
			_statsLabel.text =
				$"Total Archetypes: {allArchetypes.Length} | Total Entities: {totalEntities}";

			foreach (var archetype in allArchetypes)
			{
				if (!_archetypeCache.TryGetValue(archetype.Id, out var ui))
				{
					ui = CreateArchetypeUI(archetype);
					_archetypeCache.Add(archetype.Id, ui);
					_scrollView.Add(ui.Root);
					ApplyFilteringToArchetype(ui);
				}

				var entities = componentManager.GetEntities(archetype);
				ui.Title.text = $"Archetype {archetype.Id}";
				ui.Foldout.text = $"Entities (Count: {entities.Length})";

				if (ui.Foldout.value) // Only update entities if foldout is open
				{
					UpdateEntityList(ui, entities, world);
				}
			}
		}

		private ArchetypeUI CreateArchetypeUI(Archetype archetype)
		{
			var ui = new ArchetypeUI
			{
				Root = new VisualElement
				{
					style =
					{
						marginBottom = 2,
						marginTop = 2,
						backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.4f),
						borderBottomWidth = 1,
						borderBottomColor = new Color(0.15f, 0.15f, 0.15f),
						paddingBottom = 5
					}
				},
				Title = new Label($"Archetype {archetype.Id}")
				{
					style =
					{
						unityFontStyleAndWeight = FontStyle.Bold,
						fontSize = 13,
						paddingLeft = 5,
						paddingTop = 5,
						paddingBottom = 2
					}
				},
				Foldout = new Foldout
				{
					value = true, // Default expanded
					text = "Entities"
				}
			};

			ui.Root.Add(ui.Title);

			var components = archetype.GetComponents();

			// Required Components
			var requiredHeader = new VisualElement
			{
				style =
				{
					flexDirection = FlexDirection.Row,
					alignItems = Align.Center,
					marginTop = 5,
					paddingLeft = 5
				}
			};
			requiredHeader.Add(
				new Label("Required: ")
					{ style = { unityFontStyleAndWeight = FontStyle.Bold, width = 70 } });
			ui.RequiredContainer = new VisualElement
			{
				style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, flexGrow = 1 }
			};
			requiredHeader.Add(ui.RequiredContainer);
			ui.Root.Add(requiredHeader);

			foreach (var c in components.Required)
			{
				var name = _componentNames.Has(c) ? _componentNames.Get(c) : $"Unknown ({c})";
				ui.RequiredContainer.Add(CreateComponentPill(name, true));
			}

			// Excluded Components
			var excludedHeader = new VisualElement
			{
				style =
				{
					flexDirection = FlexDirection.Row,
					alignItems = Align.Center,
					marginTop = 2,
					paddingLeft = 5
				}
			};
			excludedHeader.Add(
				new Label("Excluded: ")
					{ style = { unityFontStyleAndWeight = FontStyle.Bold, width = 70 } });
			ui.ExcludedContainer = new VisualElement
			{
				style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, flexGrow = 1 }
			};
			excludedHeader.Add(ui.ExcludedContainer);
			ui.Root.Add(excludedHeader);

			foreach (var c in components.Excluded)
			{
				var name = _componentNames.Has(c) ? _componentNames.Get(c) : $"Unknown ({c})";
				ui.ExcludedContainer.Add(CreateComponentPill(name, false));
			}

			ui.Root.Add(ui.Foldout);

			ui.EntityList = new VisualElement { style = { paddingLeft = 15 } };
			ui.Foldout.Add(ui.EntityList);

			return ui;
		}

		private VisualElement CreateComponentPill(string text, bool isRequired)
		{
			var pill = new Button(() => { _searchField.value = text; })
			{
				text = text,
				style =
				{
					fontSize = 10,
					marginRight = 4,
					marginBottom = 2,
					paddingLeft = 4,
					paddingRight = 4,
					paddingTop = 0,
					paddingBottom = 0,
					borderTopLeftRadius = 8,
					borderTopRightRadius = 8,
					borderBottomLeftRadius = 8,
					borderBottomRightRadius = 8,
					backgroundColor = isRequired
						? new Color(0.15f, 0.4f, 0.15f)
						: new Color(0.4f, 0.15f, 0.15f),
					color = Color.white
				}
			};
			pill.AddToClassList("unity-button--mini");
			return pill;
		}

		private void UpdateEntityList(ArchetypeUI ui, ReadOnlySpan<Entity> entities, World world)
		{
			// Re-use existing buttons
			var i = 0;
			for (; i < entities.Length; i++)
			{
				var entity = entities[i];
				world.TryGetGameObject(entity, out var gameObject);
				var label = gameObject != null ? gameObject.name : entity.ToString();

				if (i < ui.EntityButtons.Count)
				{
					// If the entity is different, update the button
					if (i >= ui.Entities.Count || !ui.Entities[i].Equals(entity))
					{
						var btn = ui.EntityButtons[i];
						btn.text = label;
						btn.clickable = new Clickable(() => OnEntityButtonClicked(gameObject));

						if (i < ui.Entities.Count)
						{
							ui.Entities[i] = entity;
						}
						else
						{
							ui.Entities.Add(entity);
						}
					}
					else
					{
						// Same entity, but GameObject name might have changed
						if (ui.EntityButtons[i].text != label)
						{
							ui.EntityButtons[i].text = label;
						}
					}
				}
				else
				{
					// Add new button
					var btn = GetButtonFromPool(label, gameObject);
					ui.EntityList.Add(btn);
					ui.EntityButtons.Add(btn);
					ui.Entities.Add(entity);
				}
			}

			// Remove excess buttons
			while (ui.EntityButtons.Count > entities.Length)
			{
				var lastIdx = ui.EntityButtons.Count - 1;
				var btn = ui.EntityButtons[lastIdx];
				ui.EntityList.Remove(btn);
				_buttonPool.Add(btn);
				ui.EntityButtons.RemoveAt(lastIdx);
				ui.Entities.RemoveAt(lastIdx);
			}

			ui.LastEntityCount = entities.Length;
		}

		private Button GetButtonFromPool(string label, GameObject gameObject)
		{
			Button btn;
			if (_buttonPool.Count > 0)
			{
				btn = _buttonPool[^1];
				_buttonPool.RemoveAt(_buttonPool.Count - 1);
				btn.text = label;
				// Reset callback
				btn.clickable = new Clickable(() => OnEntityButtonClicked(gameObject));
			}
			else
			{
				btn = new Button(() => OnEntityButtonClicked(gameObject))
				{
					style =
					{
						unityTextAlign = TextAnchor.MiddleLeft,
						marginTop = 0, marginBottom = 0,
						paddingTop = 0, paddingBottom = 0
					}
				};
				btn.AddToClassList("unity-button--mini");
			}

			btn.text = label;
			return btn;
		}

		private void OnEntityButtonClicked(GameObject gameObject)
		{
			if (gameObject != null)
			{
				Selection.activeGameObject = gameObject;
				EditorGUIUtility.PingObject(gameObject);
			}
		}

		private void ApplyFiltering()
		{
			foreach (var ui in _archetypeCache.Values)
			{
				ApplyFilteringToArchetype(ui);
			}
		}

		private void ApplyFilteringToArchetype(ArchetypeUI ui)
		{
			if (string.IsNullOrEmpty(_searchFilter))
			{
				ui.Root.style.display = DisplayStyle.Flex;
				return;
			}

			bool MatchContainer(VisualElement container)
			{
				return container.Children().Any(child =>
					child is Button btn && btn.text.IndexOf(
						_searchFilter,
						StringComparison.OrdinalIgnoreCase) >= 0);
			}

			var match = MatchContainer(ui.RequiredContainer) ||
				MatchContainer(ui.ExcludedContainer) ||
				ui.Title.text.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0;

			ui.Root.style.display = match ? DisplayStyle.Flex : DisplayStyle.None;
		}
	}
}
#endif