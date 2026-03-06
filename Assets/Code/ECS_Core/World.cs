using System;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using Beneton.ECS.Core.Editor;
#endif

namespace Beneton.ECS.Core
{
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

		public ComponentManager ComponentManager { get; }

		public World()
		{
			ComponentManager = new ComponentManager(this);

#if UNITY_EDITOR
			var debugGo = new GameObject("ECS-Debug");
			var debugRef = debugGo.AddComponent<ECSDebugRef>();
			debugRef.World = this;
			debugRef.ComponentManager = ComponentManager;
			ComponentManager.TryFindTimelineHandler();
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
			var pair = new SystemCommandBufferPair
			{
				System = system,
				CommandBuffer = new CommandBuffer<T>()
			};

			systemCollection.Set(_systemId, pair);
			system.OnCreate(ComponentManager);
			_systemId++;

			return system;
		}

		public void Bake(Baker baker)
		{
			var entity = GetOrCreateEntity(baker.gameObject);
			baker.Bake(entity, ComponentManager, this);
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
			_entityId++;
			_entities.Set(entity, entity);
			return entity;
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
			var instance = Object.Instantiate(spawnerPrefab, parent);
			instance.name = instance.name[..^"(Clone)".Length];
			var entity = GetOrCreateEntity(instance);

			var bakers = instance.GetComponentsInChildren<Baker>(false);
			foreach (var baker in bakers)
			{
				baker.Bake(entity, ComponentManager, this);
			}

			return (entity, instance);
		}

		public void DestroyEntity(Entity entity)
		{
			_entities.Remove(entity);
			ComponentManager.RemoveAllComponents(entity);
			_entityGameObjectLookup.Remove(entity);
		}

		public bool HasEntity(Entity entity)
		{
			return _entities.Has(entity);
		}

		public ReadOnlySpan<Entity> GetEntities()
		{
			return _entities.Values;
		}

		public void Start()
		{
			ComponentManager.UpdateArchetypes();
		}

		public void Update(float deltaTime)
		{
			foreach (var pair in _systems.Values)
			{
				pair.System.CleanUp(ComponentManager, pair.CommandBuffer);
				pair.CommandBuffer.Execute(ComponentManager);

				pair.System.Update(deltaTime, ComponentManager, pair.CommandBuffer, this);
				pair.CommandBuffer.Execute(ComponentManager);
			}
		}

		public void LateUpdate(float deltaTime)
		{
			foreach (var pair in _lateSystems.Values)
			{
				pair.System.CleanUp(ComponentManager, pair.CommandBuffer);
				pair.CommandBuffer.Execute(ComponentManager);

				pair.System.Update(deltaTime, ComponentManager, pair.CommandBuffer, this);
				pair.CommandBuffer.Execute(ComponentManager);
			}
		}
	}
}