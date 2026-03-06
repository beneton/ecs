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
		private Camera _camera;
		private readonly RaycastHit[] _hits = new RaycastHit[1];
		private readonly List<RaycastResult> _uiHits = new(10);

		private InputAction _attackAction;
		private InputAction _pointPosition;
		private EventSystem _eventSystem;

		public override void OnCreate(IArchetypeProvider archetypeProvider)
		{
			_camera = Camera.main;
			_attackAction = InputSystem.actions.FindAction("Player/Attack");
			_pointPosition = InputSystem.actions.FindAction("UI/Point");

			_eventSystem = EventSystem.current;
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

			// Try to find click on UI Entity
			if (_eventSystem.IsPointerOverGameObject())
			{
				_eventSystem.RaycastAll(
					new PointerEventData(_eventSystem)
					{
						position = mousePosition
					},
					_uiHits);

				foreach (var uiHit in _uiHits)
				{
					// Add interaction to the first Entity found
					if (world.TryGetEntity(uiHit.gameObject, out var entity))
					{
						commandBuffer.AddComponent(entity, new Clicked());
						return;
					}
				}

				return;
			}

			// Try to find click on World Entity
			var ray = _camera.ScreenPointToRay(mousePosition);
			if (Physics.RaycastNonAlloc(ray, _hits, 100) > 0)
			{
				var hitGameObject = _hits[0].transform.gameObject;
				if (world.TryGetEntity(hitGameObject, out var entity))
				{
					commandBuffer.AddComponent(entity, new Clicked());
				}
			}
		}
	}
}