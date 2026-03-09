using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScene : MonoBehaviour
{
	[SerializeField]
	private Button _loadSceneButton;

	[SerializeField]
	private int _sceneIndex = 0;

	private void Awake()
	{
		_loadSceneButton.onClick.AddListener(OnButtonClick);
	}

	private void OnButtonClick()
	{
		SceneManager.LoadScene(_sceneIndex, LoadSceneMode.Single);
	}
}