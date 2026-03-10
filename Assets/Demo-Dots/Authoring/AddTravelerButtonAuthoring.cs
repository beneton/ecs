using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace DotsDemo
{
	public class AddTravelerButtonAuthoring : MonoBehaviour
	{
		[SerializeField]
		private Button _button;

		[SerializeField]
		private int _count = 1;

		private void Awake()
		{
			_button.onClick.AddListener(OnButtonClick);
		}

		private void OnButtonClick()
		{
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			var requestEntity = entityManager.CreateEntity();
			entityManager.AddComponentData(
				requestEntity,
				new SpawnTravelerRequest
				{
					Amount = _count
				});
		}
	}
}