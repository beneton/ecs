using UnityEngine;
using UnityEngine.UI;

namespace OOPDemo
{
	public class AddTravelerButton : MonoBehaviour
	{
		[SerializeField]
		private Button _button;

		[SerializeField]
		private GameObject _travelerPrefab;

		[SerializeField]
		private Transform _travelerContainer;

		[SerializeField]
		private int _count = 1;

		private void Awake()
		{
			_button.onClick.AddListener(OnButtonClick);
		}

		private void OnButtonClick()
		{
			for (var i = 0; i < _count; i++)
			{
				var newTraveler = Instantiate(_travelerPrefab, _travelerContainer);
				newTraveler.transform.position =
					new Vector3(
						Random.Range(-10f, 10f),
						0.5f,
						Random.Range(-10f, 10f));
			}
		}
	}
}