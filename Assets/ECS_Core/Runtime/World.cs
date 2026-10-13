using System;
using System.Collections.Generic;
using System.Linq;
using Beneton.ECS.Core.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Beneton.ECS.Core
{
	/// <summary>
	/// Represents the main container and orchestrator for the Entity Component System (ECS).
	/// - Manages the lifecycle of entities
	/// - Handles system registration and execution 
	/// - Provides integration with Unity GameObjects and Bakers.
	/// </summary>
	public class World : IWorld
	{
		private class SystemCommandBufferPair
		{
			public BaseSystem System;
			public ICommandBuffer CommandBuffer;
		}

		// Ids are SystemIds
		private readonly SparseSet<SystemCommandBufferPair> _systems = new();
		private readonly SparseSet<SystemCommandBufferPair> _lateSystems = new();

		private readonly SparseSet<Entity> _entities = new();
		private readonly SparseSet<GameObject> _entityGameObjectLookup = new();

		private int _entityId = Entity.NullId + 1;
		private int _systemId = 1;

		private readonly ComponentManager _componentManager;
		internal ComponentManager ComponentManager => _componentManager;

		private readonly FindObjectsInactive _inactiveObjectsPolicy;

		private ITimelineHandler _timelineHandler;

		public World(bool considerInactiveGameObjects = false)
		{
			_inactiveObjectsPolicy = considerInactiveGameObjects
				? FindObjectsInactive.Include
				: FindObjectsInactive.Exclude;
			_componentManager = new ComponentManager(this);
			_timelineHandler = new StubTimelineHandler();

#if UNITY_EDITOR
			// Needed support object to enable debug inspector windows
			var inspectorHelperGo = new GameObject("ECS-Inspector-Helper");
			var inspectorHelper = inspectorHelperGo.AddComponent<EcsInspectorsHelper>();
			inspectorHelper.World = this;
			inspectorHelper.ComponentManager = _componentManager;

			var ecsTimeline = Resources.FindObjectsOfTypeAll<EcsTimeline>();
			if (ecsTimeline is { Length: > 0 })
			{
				SetTimelineHandler(ecsTimeline[0]);
			}
#endif
		}

		internal void SetTimelineHandler(ITimelineHandler timelineHandler)
		{
			_timelineHandler = timelineHandler;
		}

		public T AddSystem<T>() where T : BaseSystem, new()
		{
			return AddSystem(new T(), _systems);
		}

		public T AddSystem<T>(T system) where T : BaseSystem
		{
			return AddSystem(system, _systems);
		}

		public T AddSystemLate<T>() where T : BaseSystem, new()
		{
			return AddSystem(new T(), _lateSystems);
		}

		public T AddSystemLate<T>(T system) where T : BaseSystem
		{
			return AddSystem(system, _lateSystems);
		}

		private T AddSystem<T>(T system, SparseSet<SystemCommandBufferPair> systemCollection)
			where T : BaseSystem
		{
			var commandBuffer = new CommandBuffer<T>();
			var pair = new SystemCommandBufferPair
			{
				System = system,
				CommandBuffer = commandBuffer
			};

			systemCollection.Set(_systemId, pair);
			system.OnCreate(_componentManager);
			_systemId++;

			return system;
		}

		public Entity CreateEntity(string entityName)
		{
			if (Application.isEditor)
			{
				var trackingGo = new GameObject(entityName);
				return GetOrCreateEntity(trackingGo);
			}
			else
			{
				return InternalCreateEntity();
			}
		}

		public Entity GetOrCreateEntity(GameObject gameObject)
		{
			for (var i = 0; i < _entityGameObjectLookup.Values.Length; i++)
			{
				var existingGo = _entityGameObjectLookup.Values[i];
				if (existingGo == gameObject)
				{
					return new Entity(_entityGameObjectLookup.Keys[i]);
				}
			}

			var entity = InternalCreateEntity();
			_entityGameObjectLookup.Set(entity, gameObject);
			gameObject.name += $" [{entity.Id.ToString()}]";

			_timelineHandler.RegisterEntityCreation(entity, this);
			return entity;
		}

		private Entity InternalCreateEntity()
		{
			var entity = new Entity(_entityId);
			_entities.Set(entity, entity);
			_entityId++;
			return entity;
		}

		public bool HasEntity(Entity entity)
		{
			return _entities.Has(entity);
		}

		public bool TryGetEntity(GameObject gameObject, out Entity entity)
		{
			for (var i = 0; i < _entityGameObjectLookup.Values.Length; i++)
			{
				var existingGo = _entityGameObjectLookup.Values[i];
				if (existingGo == gameObject)
				{
					entity = new Entity(_entityGameObjectLookup.Keys[i]);
					return true;
				}
			}

			entity = Entity.Null;
			return false;
		}

		public bool TryGetGameObject(Entity entity, out GameObject gameObject)
		{
			gameObject = _entityGameObjectLookup.TryGet(entity, out var exists);
			return exists;
		}

		/// <summary>
		/// Instantiates a prefab and turns it into entities using the same rules as <see cref="Start"/>:
		/// every GameObject in the instance that has a <see cref="Baker"/> or an <see cref="ISystemNode"/>
		/// becomes its own entity, its Bakers bake onto that entity and its nodes register against it.
		/// - <see cref="SpawnResult.Entity"/> is <see cref="Entity.Null"/> when the root GameObject is not an entity.
		/// - Destroying the root entity does not destroy the child entities; the caller is responsible for them.
		/// </summary>
		public SpawnResult Spawn(
			GameObject spawnerPrefab,
			Transform parent)
		{
			// Create new Instance
			var instance = Object.Instantiate(spawnerPrefab, parent);
			instance.name = instance.name[..^"(Clone)".Length];

			var includeInactive = _inactiveObjectsPolicy == FindObjectsInactive.Include;
			var rootEntity = Entity.Null;
			List<(Entity Entity, GameObject GameObject)> children = null;
			var hasBakers = false;

			foreach (var transform in instance.GetComponentsInChildren<Transform>(includeInactive))
			{
				var gameObject = transform.gameObject;
				var entity = BakeAndRegisterNodes(gameObject, ref hasBakers);

				if (entity.IsNull)
				{
					continue;
				}

				if (gameObject == instance)
				{
					rootEntity = entity;
				}
				else
				{
					children ??= new List<(Entity Entity, GameObject GameObject)>();
					children.Add((entity, gameObject));
				}
			}

			if (hasBakers)
			{
				_componentManager.UpdateArchetypes();
			}

			return new SpawnResult(rootEntity, instance, children);
		}

		/// <summary>
		/// Turns a GameObject into an entity if it has any <see cref="Baker"/> or <see cref="ISystemNode"/>:
		/// runs its Bakers on that entity and registers its nodes against it.
		/// Shared by <see cref="Start"/> and <see cref="Spawn"/> so both follow the same rules.
		/// </summary>
		/// <returns>The entity, or <see cref="Entity.Null"/> if the GameObject has neither.</returns>
		private Entity BakeAndRegisterNodes(GameObject gameObject, ref bool hasBakers)
		{
			var bakers = gameObject.GetComponents<Baker>();
			var nodes = gameObject.GetComponents<ISystemNode>();

			if (bakers.Length == 0 && nodes.Length == 0)
			{
				return Entity.Null;
			}

			var entity = GetOrCreateEntity(gameObject);

			foreach (var baker in bakers)
			{
				_timelineHandler.SetExecutingBaker(baker);
				baker.Bake(entity, _componentManager, this);
			}

			_timelineHandler.SetExecutingBaker(null);
			hasBakers |= bakers.Length > 0;

			RegisterSystemNodes(nodes, entity);

			return entity;
		}

		public ReadOnlySpan<Entity> GetEntities()
		{
			return _entities.Values;
		}

		/// <summary>
		/// Destroys the entity immediately. Systems and system nodes must use
		/// <see cref="ICommandBuffer.DestroyEntity"/> instead, so entities are never
		/// removed from an archetype while it is being iterated.
		/// </summary>
		public void DestroyEntity(Entity entity)
		{
			if (!HasEntity(entity))
			{
				return;
			}

			_timelineHandler.RegisterEntityDestruction(entity, this);

			_entities.Remove(entity);
			_componentManager.RemoveAllComponents(entity);
			_entityGameObjectLookup.Remove(entity);

			foreach (var pair in _systems.Values)
			{
				UnregisterNodeIfNeeded(pair);
			}

			foreach (var pair in _lateSystems.Values)
			{
				UnregisterNodeIfNeeded(pair);
			}

			return;

			void UnregisterNodeIfNeeded(SystemCommandBufferPair pair)
			{
				if (pair.System is IDistributedSystem distSystem)
				{
					distSystem.UnregisterNode(entity);
				}
			}
		}

		private void RegisterSystemNodes(ISystemNode[] nodes, Entity entity)
		{
			foreach (var node in nodes)
			{
				foreach (var pair in _systems.Values)
				{
					RegisterNodeIfFound(pair, node);
				}

				foreach (var pair in _lateSystems.Values)
				{
					RegisterNodeIfFound(pair, node);
				}
			}

			return;

			void RegisterNodeIfFound(SystemCommandBufferPair pair, ISystemNode node)
			{
				if (pair.System is IDistributedSystem distSystem &&
					distSystem.SystemType == node.SystemType)
				{
					distSystem.RegisterNode(node, entity);
				}
			}
		}

		public void Start()
		{
			_timelineHandler.SetExecutingSystem("World Start");

			// Every GameObject with a Baker or a SystemNode becomes an entity:
			// its Bakers bake onto it and its SystemNodes register against it
			var hasBakers = false;
			var allTransforms = Object.FindObjectsByType<Transform>(_inactiveObjectsPolicy);

			foreach (var transform in allTransforms)
			{
				BakeAndRegisterNodes(transform.gameObject, ref hasBakers);
			}

			_componentManager.UpdateArchetypes();
		}

		public void Update(float deltaTime)
		{
			foreach (var pair in _systems.Values)
			{
				_timelineHandler.SetExecutingSystem(pair.System);
				pair.System.Update(deltaTime, _componentManager, pair.CommandBuffer, this);
				pair.CommandBuffer.Execute(this, _componentManager);
			}
		}

		public void LateUpdate(float deltaTime)
		{
			foreach (var pair in _lateSystems.Values)
			{
				_timelineHandler.SetExecutingSystem(pair.System);
				pair.System.Update(deltaTime, _componentManager, pair.CommandBuffer, this);
				pair.CommandBuffer.Execute(this, _componentManager);
			}
		}

		internal void RegisterAddComponent(Entity entity, int componentId)
		{
			_timelineHandler.RegisterAddComponent(entity, this, componentId);
		}

		internal void RegisterUpdateComponent(Entity entity, int componentId)
		{
			_timelineHandler.RegisterUpdateComponent(entity, this, componentId);
		}

		internal void RegisterRemoveComponent(Entity entity, int componentId)
		{
			_timelineHandler.RegisterRemoveComponent(entity, this, componentId);
		}

		internal void RegisterRemoveAllComponents(Entity entity)
		{
			_timelineHandler.RegisterRemoveAllComponents(entity, this);
		}
	}
}