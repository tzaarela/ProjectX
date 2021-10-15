using Mirror;

namespace Managers
{
	public class RoundManager : NetworkBehaviour
	{

		private int numberOfActivePlayers;
		
		private static bool hasBeenProvided;

		public int NumberOfActivePlayers => numberOfActivePlayers;

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

		public void AddActivePlayer()
		{
			numberOfActivePlayers++;
			print("NumberOfActivePlayers = " + numberOfActivePlayers);
		}
	}
}