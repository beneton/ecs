using System;
using UnityEngine;

namespace Beneton.ECS.Core
{
	public interface IWorld
	{
		(Entity Entity, GameObject GameObject) Spawn(GameObject spawnerPrefab, Transform parent);
		Entity CreateEntity(string entityName);
		Entity GetOrCreateEntity(GameObject gameObject);
		bool TryGetEntity(GameObject gameObject, out Entity entity);
		bool TryGetGameObject(Entity entity, out GameObject gameObject);
		void DestroyEntity(Entity entity);
		ReadOnlySpan<Entity> GetEntities();
	}
}