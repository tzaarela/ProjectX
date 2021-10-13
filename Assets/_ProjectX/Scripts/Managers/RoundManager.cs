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
				print("RoundManager provided to ServiceLocator");
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