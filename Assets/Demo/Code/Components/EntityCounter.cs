using Beneton.ECS.Core;
using TMPro;

namespace ECSSample.Components
{
	public partial struct EntityCounter : ISingletonComponent
	{
		public TextMeshProUGUI TextField;
	}
}