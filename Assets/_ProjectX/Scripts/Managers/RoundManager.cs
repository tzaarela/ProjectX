using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Mirror;

namespace Managers
{
	public class RoundManager : NetworkBehaviour, ISendGlobalSignal
	{
		private int numberOfSpawnedPlayers;

		private static bool hasBeenProvided;

		public int NumberOfConnectedClients { get; set; }

		// NetworkIdentity = ServerOnly

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
			numberOfSpawnedPlayers++;
			print("NumberOfSpawnedPlayers = " + numberOfSpawnedPlayers);
			if (numberOfSpawnedPlayers == NumberOfConnectedClients)
			{
				print("Mediator: All clients connected to game!");
				SendGlobal(GlobalEvent.ALL_PLAYERS_CONECTED_TO_GAME);
			}
		}

		public void SendGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			GlobalMediator.Instance.ReceiveGlobal(eventState, globalSignalData);
		}
	}
}