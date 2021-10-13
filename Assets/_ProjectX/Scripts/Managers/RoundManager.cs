using Mirror;

namespace Managers
{
	public class RoundManager : NetworkBehaviour
	{
		private static bool hasBeenProvided;
		
		private void Awake()
		{
			if (!hasBeenProvided)
			{
				ServiceLocator.ProvideRoundManager(this);
				hasBeenProvided = true;
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(gameObject);
			}
		}
	}
}