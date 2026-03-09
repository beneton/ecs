using System;
using TMPro;
using UnityEngine;

namespace OOPDemo
{
	public class EntityCounter : MonoBehaviour
	{
		private static EntityCounter _instance;
		public static EntityCounter Instance => _instance;

		[SerializeField]
		private TextMeshProUGUI _countField;

		private int _count;
		private bool _hasChange;

		private void Awake()
		{
			_instance = this;
		}

		public void Increase()
		{
			_count++;
			_hasChange = true;
		}

		public void Decrease()
		{
			_count--;
			_hasChange = true;
		}

		private void Update()
		{
			if (_hasChange)
			{
				_hasChange = false;
				_countField.text = _count.ToString();
			}
		}
	}
}