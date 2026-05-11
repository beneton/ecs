using System;
using UnityEngine;

namespace Beneton.ECS.Core
{
	/// <summary>
	/// Defines the core contract for the ECS World, providing an abstraction layer for entity management and Unity integration.
	/// - Intent: Exposes only methods that are safe to be used by Systems and Bakers
	/// - Responsibilities: Handles entity creation, destruction, and mapping between ECS entities and Unity GameObjects.
	/// </summary>
	public interface IWorld
	{
		Entity CreateEntity(string entityName);
		Entity GetOrCreateEntity(GameObject gameObject);
		bool TryGetEntity(GameObject gameObject, out Entity entity);
		bool HasEntity(Entity entity);
		bool TryGetGameObject(Entity entity, out GameObject gameObject);
		void DestroyEntity(Entity entity);
		ReadOnlySpan<Entity> GetEntities();
		(Entity Entity, GameObject GameObject) Spawn(GameObject spawnerPrefab, Transform parent);
	}
}