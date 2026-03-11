using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using Beneton.ECS.Core.Editor;
#endif

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

		public World()
		{
			_componentManager = new ComponentManager(this);

#if UNITY_EDITOR
			// Needed support object to enable debug inspector windows
			var debugGo = new GameObject("ECS-Debug");
			var debugRef = debugGo.AddComponent<EcsDebugRef>();
			debugRef.World = this;
			debugRef.ComponentManager = _componentManager;
			_componentManager.TryFindTimelineHandler();
#endif
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

		public void Bake(Baker baker)
		{
			var entity = GetOrCreateEntity(baker.gameObject);
#if UNITY_EDITOR
			_componentManager.CurrentExecutingBaker = baker.GetType().Name;
#endif
			baker.Bake(entity, _componentManager, this);
#if UNITY_EDITOR
			_componentManager.CurrentExecutingBaker = string.Empty;
#endif
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

		public (Entity Entity, GameObject GameObject) Spawn(
			GameObject spawnerPrefab,
			Transform parent)
		{
			// Create new Instance
			var instance = Object.Instantiate(spawnerPrefab, parent);
			instance.name = instance.name[..^"(Clone)".Length];
			var entity = GetOrCreateEntity(instance);

			// Run Bakers on the instance
			var bakers = instance.GetComponentsInChildren<Baker>(false);
			foreach (var baker in bakers)
			{
#if UNITY_EDITOR
				_componentManager.CurrentExecutingBaker = baker.GetType().Name;
#endif
				baker.Bake(entity, _componentManager, this);
			}
#if UNITY_EDITOR
			_componentManager.CurrentExecutingBaker = string.Empty;
#endif

			// Register any SystemNode present on the Instance
			var allNodes = instance.GetComponentsInChildren<ISystemNode>();
			RegisterSystemNodes(allNodes, entity);

			return (entity, instance);
		}

		public ReadOnlySpan<Entity> GetEntities()
		{
			return _entities.Values;
		}

		public void DestroyEntity(Entity entity)
		{
			_entities.Remove(entity);
			_componentManager.RemoveAllComponents(entity);
			_entityGameObjectLookup.Remove(entity);

			foreach (var pair in _systems.Values)
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
					if (pair.System is IDistributedSystem distSystem &&
						distSystem.SystemType == node.SystemType)
					{
						distSystem.RegisterNode(node, entity);
					}
				}
			}
		}

		public void Start(FindObjectsInactive inactiveObjectsPolicy)
		{
			// Find all existing Bakers and Bake them
			var allBakers = Object.FindObjectsByType<Baker>(
				inactiveObjectsPolicy,
				FindObjectsSortMode.None);

			foreach (var baker in allBakers)
			{
				Bake(baker);
			}

			// Find all SystemNodes and Register them
			var allTransforms = Object.FindObjectsByType<Transform>(
					inactiveObjectsPolicy,
					FindObjectsSortMode.None)
				.ToArray();

			foreach (var transform in allTransforms)
			{
				var gameObject = transform.gameObject;
				var allNodes = gameObject.GetComponentsInChildren<ISystemNode>();
				if (allNodes.Length > 0)
				{
					var entity = GetOrCreateEntity(gameObject);
					RegisterSystemNodes(allNodes, entity);
				}
			}

			_componentManager.UpdateArchetypes();
		}

		public void Update(float deltaTime)
		{
			foreach (var pair in _systems.Values)
			{
#if UNITY_EDITOR
				_componentManager.CurrentExecutingSystem = pair.System.GetType().Name;
#endif
				pair.System.Update(deltaTime, _componentManager, pair.CommandBuffer, this);
				pair.CommandBuffer.Execute(_componentManager);
			}
		}

		public void LateUpdate(float deltaTime)
		{
			foreach (var pair in _lateSystems.Values)
			{
#if UNITY_EDITOR
				_componentManager.CurrentExecutingSystem = pair.System.GetType().Name;
#endif
				pair.System.Update(deltaTime, _componentManager, pair.CommandBuffer, this);
				pair.CommandBuffer.Execute(_componentManager);
			}
		}
	}
}