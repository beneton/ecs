using Beneton.ECS.Core;
using TMPro;

namespace ECS.Demo.Basic.Components
{
	/// <summary>
	/// A singleton component that provides a reference to a UI text field for displaying the total entity count.
	/// </summary>
	public partial struct EntityCounter : ISingletonComponent
	{
		public TextMeshProUGUI TextField;
	}
}