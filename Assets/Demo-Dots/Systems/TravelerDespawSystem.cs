using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DotsDemo
{
	public partial class TravelerDespawSystem : SystemBase
	{
		private InputAction _interact;
		private InputAction _mousePosition;
		private Camera _camera;

		protected override void OnCreate()
		{
			RequireForUpdate<PhysicsWorldSingleton>();

			_interact = InputSystem.actions.FindAction("Player/Attack");
			_mousePosition = InputSystem.actions.FindAction("UI/Point");
		}

		protected override void OnUpdate()
		{
			if (!_interact.WasReleasedThisFrame())
			{
				return;
			}

			_camera ??= Camera.main;
			if (_camera == null)
			{
				Debug.LogError("No camera found!");
				return;
			}

			var mouseScreenPosition = _mousePosition.ReadValue<Vector2>();
			var ray = _camera.ScreenPointToRay(mouseScreenPosition);
			float3 start = ray.origin;
			float3 end = ray.origin + ray.direction * 1000f;

			var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
			var collisionWorld = physicsWorld.CollisionWorld;

			var input = new RaycastInput
			{
				Start = start,
				End = end,
				Filter = CollisionFilter.Default
			};

			if (collisionWorld.CastRay(input, out var hit))
			{
				DestroyEntityAndChildren(hit.Entity);
			}
		}

		private void DestroyEntityAndChildren(Entity entity)
		{
			var entitiesToDestroy = new List<Entity>(8);
			CollectChildrenRecursively(entity, entitiesToDestroy);

			foreach (var entityToDestroy in entitiesToDestroy)
			{
				EntityManager.DestroyEntity(entityToDestroy);
			}
		}

		private void CollectChildrenRecursively(Entity entity, List<Entity> toDestroy)
		{
			toDestroy.Add(entity);
			if (EntityManager.HasBuffer<Child>(entity))
			{
				var children = EntityManager.GetBuffer<Child>(entity);
				foreach (var child in children)
				{
					CollectChildrenRecursively(child.Value, toDestroy);
				}
			}
		}
	}
}