using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using Beneton.ECS.Core.Editor;
#endif

namespace Beneton.ECS.Core
{
	public class ComponentManager : IArchetypeProvider, IComponentManager
	{
		// Key is Component Type Id
		private readonly SparseSet<IComponentStorage> _componentStorages = new();

		// Key is Archetype.Id
		private readonly SparseSet<Archetype> _archetypes = new();

		// Key is Archetype.Id
		private readonly SparseSet<SparseSet<Entity>> _entitiesInArchetype = new();

		// Key is Entity.Id
		private readonly SparseSet<Entity> _modifiedEntities = new();

		private ITimelineHandler _timelineHandler;

		private readonly World _world;
		private int _archetypeId = 1;

		internal ComponentManager(World world)
		{
			_world = world;
		}

		internal void TryFindTimelineHandler()
		{
#if UNITY_EDITOR
			var ecsTimeline = Resources.FindObjectsOfTypeAll<ECSTimeline>();
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

		public Archetype GetOrCreateArchetype(int[] required)
		{
			return GetOrCreateArchetype(required, Array.Empty<int>());
		}

		public Archetype GetOrCreateArchetype(int[] required, int[] exclude)
		{
			foreach (var archetype in _archetypes.Values)
			{
				if (archetype.Equals(required, exclude))
				{
					return archetype;
				}
			}

			var newArchetype = new Archetype(_archetypeId, required, exclude);
			_archetypeId++;
			_entitiesInArchetype.Set(newArchetype.Id, new SparseSet<Entity>());
			_archetypes.Set(newArchetype.Id, newArchetype);
			PopulateArchetype(newArchetype);
			return newArchetype;
		}

		public ReadOnlySpan<Entity> GetEntities(Archetype archetype)
		{
			return _entitiesInArchetype.Get(archetype.Id).Values;
		}

		public Entity GetFirstEntity(Archetype archetype)
		{
			var set = GetEntities(archetype);
			foreach (var entity in set)
			{
				return entity;
			}

			return Entity.Null;
		}

		public void AddComponent<T>(Entity entity, in T component) where T : struct, IComponent
		{
			var componentType = typeof(T);
			var typeId = new T().TypeId;
			if (entity.IsNull)
			{
				// ReSharper disable once Unity.PerformanceCriticalCodeInvocation
				Debug.LogError(
					$"Trying to add component to a null Entity. Component: ({componentType})");
				return;
			}

			if (!_world.HasEntity(entity))
			{
				// ReSharper disable once Unity.PerformanceCriticalCodeInvocation
				Debug.LogError(
					$"Trying to add component to an Entity that is not part of the World. Entity({entity.ToString()}), Component: ({componentType})");
				return;
			}

			if (!TryGetTypedStorage<T>(out var typedStorage))
			{
				typedStorage = new ComponentStorage<T>();
				_componentStorages.Set(typeId, typedStorage);
			}

			if (typedStorage.HasComponent(entity))
			{
				// ReSharper disable once Unity.PerformanceCriticalCodeInvocation
				Debug.LogError(
					$"{entity.ToString()} already has component {componentType}. Consider using Update instead.");
				return;
			}

			_timelineHandler?.RegisterAddComponent(entity, typeId);

			typedStorage.Set(entity, component);
			_modifiedEntities.Set(entity, entity);
		}

		public void UpdateComponent<T>(Entity entity, in T component) where T : struct, IComponent
		{
			var componentType = typeof(T);
			var typeId = new T().TypeId;
			if (entity.IsNull)
			{
				// ReSharper disable once Unity.PerformanceCriticalCodeInvocation
				Debug.LogError(
					$"Trying to update a component in a null Entity. Component: ({componentType})");
				return;
			}

			if (!_world.HasEntity(entity))
			{
				// ReSharper disable once Unity.PerformanceCriticalCodeInvocation
				Debug.LogError(
					$"Trying to update a component in an Entity that is not part of the World. Entity({entity.ToString()}), Component: ({componentType})");
				return;
			}

			if (!TryGetTypedStorage<T>(out var typedStorage) || !typedStorage.HasComponent(entity))
			{
				// ReSharper disable once Unity.PerformanceCriticalCodeInvocation
				Debug.LogError(
					$"Trying to update a component in an Entity, but the Entity doesn't have that component. Entity({entity.ToString()}), Component: ({componentType})");
				return;
			}

			_timelineHandler?.RegisterUpdateComponent(entity, typeId);

			typedStorage.Set(entity, component);
		}

		public void AddOrUpdateComponent<T>(Entity entity, in T component)
			where T : struct, IComponent
		{
			if (TryGetTypedStorage<T>(out var typedStorage))
			{
				if (typedStorage.HasComponent(entity))
				{
					UpdateComponent(entity, component);
					return;
				}
			}

			AddComponent(entity, component);
		}

		public void AddMissingComponent<T>(Entity entity, in T component)
			where T : struct, IComponent
		{
			if (TryGetTypedStorage<T>(out var typedStorage))
			{
				if (typedStorage.HasComponent(entity))
				{
					return;
				}
			}

			AddComponent(entity, component);
		}

		internal void RemoveAllComponents(Entity entity)
		{
			foreach (var componentStorage in _componentStorages.Values)
			{
				if (componentStorage.HasComponent(entity))
				{
					componentStorage.Remove(entity);
				}
			}

			_modifiedEntities.Set(entity, entity);

			_timelineHandler?.RegisterRemoveAllComponent(entity);
			UpdateArchetypes(entity);
		}

		public void RemoveComponent<T>(Entity entity) where T : struct, IComponent
		{
			var componentType = typeof(T);
			var typeId = new T().TypeId;
			if (entity.IsNull)
			{
				// ReSharper disable once Unity.PerformanceCriticalCodeInvocation
				Debug.LogError(
					$"Trying to remove component from a null Entity. Component: ({componentType})");
				return;
			}

			if (!_world.HasEntity(entity))
			{
				// ReSharper disable once Unity.PerformanceCriticalCodeInvocation
				Debug.LogError(
					$"Trying to remove component from an Entity that is not part of the World. Entity({entity.ToString()}), Component: ({componentType})");
				return;
			}

			if (TryGetTypedStorage<T>(out var typedStorage))
			{
				_timelineHandler?.RegisterRemoveComponent(entity, typeId);

				typedStorage.Remove(entity);
				_modifiedEntities.Set(entity, entity);
			}
		}

		public T GetComponent<T>(Entity entity) where T : struct, IComponent
		{
			var typeId = new T().TypeId;
			var storage = _componentStorages.Get(typeId) as ComponentStorage<T>;
			return storage!.Get(entity);
		}

		public bool TryGetComponent<T>(Entity entity, out T component)
			where T : struct, IComponent
		{
			var componentType = typeof(T);
			if (entity.IsNull)
			{
				// ReSharper disable once Unity.PerformanceCriticalCodeInvocation
				Debug.LogError(
					$"Trying to find component in a null Entity. Component: ({componentType})");
				component = default;
				return false;
			}

			if (!_world.HasEntity(entity))
			{
				// ReSharper disable once Unity.PerformanceCriticalCodeInvocation
				Debug.LogError(
					$"Trying to find component in an Entity that is not part of the World. Entity({entity.ToString()}), Component: ({componentType})");
				component = default;
				return false;
			}

			if (TryGetTypedStorage<T>(out var typedStorage))
			{
				return typedStorage.TryGet(entity, out component);
			}

			component = default;
			return false;
		}

		public bool HasComponent<T>(Entity entity) where T : struct, IComponent
		{
			var typeId = new T().TypeId;
			if (entity.IsNull)
			{
				// ReSharper disable once Unity.PerformanceCriticalCodeInvocation
				Debug.LogError(
					$"Trying to find component in a null Entity. Component: ({typeId})");
				return false;
			}

			if (!_world.HasEntity(entity))
			{
				// ReSharper disable once Unity.PerformanceCriticalCodeInvocation
				Debug.LogError(
					$"Trying to find component in an Entity that is not part of the World. Entity({entity.ToString()}), Component: ({typeId})");
				return false;
			}

			var storage = _componentStorages.TryGet(typeId, out var exists);
			return exists && storage.HasComponent(entity);
		}

		private bool TryGetTypedStorage<T>(out ComponentStorage<T> typedStorage)
			where T : struct, IComponent
		{
			var typeId = new T().TypeId;
			var storage = _componentStorages.TryGet(typeId, out var exists);
			if (exists)
			{
				typedStorage = (ComponentStorage<T>)storage;
				return true;
			}

			typedStorage = null;
			return false;
		}

		private void PopulateArchetype(Archetype archetype)
		{
			var allEntities = _world.GetEntities();
			var entitiesInArchetype = _entitiesInArchetype.Get(archetype.Id);

			foreach (var entity in allEntities)
			{
				if (archetype.Matches(entity, this))
				{
					entitiesInArchetype.Set(entity, entity);
				}
			}
		}

		internal void UpdateArchetypes()
		{
			foreach (var entity in _modifiedEntities.Values)
			{
				UpdateArchetypes(entity);
			}

			_modifiedEntities.Clear();
		}

		private void UpdateArchetypes(Entity entity)
		{
			foreach (var archetype in _archetypes.Values)
			{
				if (archetype.Matches(entity, this))
				{
					_entitiesInArchetype.Get(archetype.Id).Set(entity, entity);
				}
				else
				{
					_entitiesInArchetype.Get(archetype.Id).Remove(entity);
				}
			}
		}

		internal bool HasComponentFast(Entity entity, int typeId)
		{
			var storage = _componentStorages.TryGet(typeId, out var exists);
			return exists && storage.HasComponent(entity);
		}

#if UNITY_EDITOR
		internal List<IComponent> GetAllComponents(Entity entity)
		{
			var components = new List<IComponent>();
			foreach (var storage in _componentStorages.Values)
			{
				if (storage.TryGetComponentTypeless(entity, out var component))
				{
					components.Add(component);
				}
			}

			return components;
		}

		internal ReadOnlySpan<Archetype> GetAllArchetypes()
		{
			return _archetypes.Values;
		}
#endif
	}
}