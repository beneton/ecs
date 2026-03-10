using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DotsDemo
{
	public partial struct TravelerDespawSystem : ISystem
	{
		private InputAction _interact;
		private InputAction _mousePosition;

		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldSingleton>();

			var actionMap = InputSystem.actions.FindActionMap("Player");
			_interact = actionMap.FindAction("Interact");
			_mousePosition = actionMap.FindAction("Point");
		}

		public void OnUpdate(ref SystemState state)
		{
			if (!_interact.WasReleasedThisFrame())
			{
				return;
			}

			// 1. Setup Ray
			var ray = Camera.main.ScreenPointToRay(_mousePosition.ReadValue<Vector2>());
			float3 start = ray.origin;
			float3 end = ray.origin + ray.direction * 100f;

			// 2. Get Physics World
			var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
			var collisionWorld = physicsWorld.CollisionWorld;

			// 3. Perform Raycast
			var input = new RaycastInput
			{
				Start = start,
				End = end,
				Filter = CollisionFilter.Default
			};

			if (collisionWorld.CastRay(input, out var hit))
			{
				// 4. Destroy the hit entity
				state.EntityManager.DestroyEntity(hit.Entity);
			}
		}
	}
}