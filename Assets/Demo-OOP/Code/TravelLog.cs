using System;
using TMPro;
using UnityEngine;

namespace OOPDemo
{
	public class TravelLog : MonoBehaviour
	{
		private static TravelLog _instance;
		public static TravelLog Instance => _instance;
		
		[SerializeField]
		private TextMeshProUGUI _distanceField;

		private float _totalDistance;
		private bool _hasUpdate = false;

		private void Awake()
		{
			_instance = this;
		}

		public void Add(float distance)
		{
			_totalDistance += distance;
			_hasUpdate = true;
		}

		private void Update()
		{
			if (_hasUpdate)
			{
				_hasUpdate = false;
				_distanceField.text = _totalDistance.ToString("F2");
			}
		}
	}
}