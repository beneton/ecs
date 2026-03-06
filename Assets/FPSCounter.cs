using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _fpsField;

	private void Update()
	{
		_fpsField.text =
			$"FPS: {Mathf.RoundToInt(1f / Time.smoothDeltaTime)} | {Time.smoothDeltaTime * 1000:F1} ms";
	}
}