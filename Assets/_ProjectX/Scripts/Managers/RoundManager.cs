using Mirror;

namespace Managers
{
	public class RoundManager : NetworkBehaviour
	{
		private void Awake()
		{
			ServiceLocator.ProvideRoundManager(this);
			DontDestroyOnLoad(gameObject);
		}
	}
}