using System.Collections.Generic;
using Beneton.ECS.Core;
using ECSSample.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ECSSample.Systems
{
	/// <summary>
	/// Detects and register clicks by adding Clicked component
	/// </summary>
	public class InputDetectorSystem : BaseSystem
	{
		private readonly List<RaycastResult> _hits = new(10);

		private readonly InputAction _attackAction =
			InputSystem.actions.FindAction("Player/Attack");

		private readonly InputAction _pointPosition =
			InputSystem.actions.FindAction("UI/Point");

		private readonly EventSystem _eventSystem = EventSystem.current;

		public override void OnCreate(IArchetypeProvider archetypeProvider)
		{
		}

		public override void Update(
			float deltaTime,
			IComponentGetter componentManager,
			ICommandBuffer commandBuffer,
			IWorld world)
		{
			if (!_attackAction.WasReleasedThisFrame())
			{
				return;
			}

			var mousePosition = _pointPosition.ReadValue<Vector2>();
			_eventSystem.RaycastAll(
				new PointerEventData(_eventSystem)
				{
					position = mousePosition
				},
				_hits);

			foreach (var uiHit in _hits)
			{
				// Add interaction to the first Entity found
				if (world.TryGetEntity(uiHit.gameObject, out var entity))
				{
					commandBuffer.AddComponent(entity, new Clicked());
					return;
				}
			}
		}
	}
}