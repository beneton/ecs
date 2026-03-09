using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace OOPDemo
{
	public class Traveler : MonoBehaviour, IPointerClickHandler
	{
		private enum State
		{
			Resting,
			Moving
		}

		[SerializeField]
		private Material _restingMaterial;

		[SerializeField]
		private Material _movingMaterial;

		private MeshRenderer[] _meshRenderers;
		private float _travelSpeed;
		private Vector3 _movingDirection;

		private float _stateDuration;
		private State _currentState;

		private TravelLog _travelLog;
		private EntityCounter _entityCounter;

		private void Awake()
		{
			_meshRenderers = GetComponentsInChildren<MeshRenderer>();
		}

		private void Start()
		{
			_entityCounter = EntityCounter.Instance;
			_travelLog = TravelLog.Instance;

			_entityCounter.Increase();
		}

		private void OnDestroy()
		{
			if (_entityCounter != null)
			{
				_entityCounter.Decrease();
			}
		}

		private void Update()
		{
			var deltaTime = Time.deltaTime;
			_stateDuration -= deltaTime;

			switch (_currentState)
			{
				case State.Resting:
					if (_stateDuration <= 0f)
					{
						StartMoving();
					}

					break;
				case State.Moving:
					if (_stateDuration <= 0f)
					{
						StartResting();
					}
					else
					{
						var travelVector = _movingDirection * (_travelSpeed * deltaTime);
						transform.position += travelVector;

						if (_travelLog != null)
						{
							_travelLog.Add(travelVector.magnitude);
						}
					}

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void StartMoving()
		{
			_currentState = State.Moving;
			_movingDirection =
				new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
			_stateDuration = Random.Range(1f, 3f);
			_travelSpeed = Random.Range(1f, 3f);
			transform.LookAt(transform.position + _movingDirection);

			foreach (var meshRenderer in _meshRenderers)
			{
				meshRenderer.sharedMaterial = _movingMaterial;
			}
		}


		private void StartResting()
		{
			_currentState = State.Resting;
			_stateDuration = Random.Range(1f, 3f);

			foreach (var meshRenderer in _meshRenderers)
			{
				meshRenderer.sharedMaterial = _restingMaterial;
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			Destroy(gameObject);
		}
	}
}