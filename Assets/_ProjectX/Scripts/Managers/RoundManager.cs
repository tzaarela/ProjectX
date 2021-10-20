using System.Collections.Generic;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Mirror;

namespace Managers
{
	public class RoundManager : NetworkBehaviour, ISendGlobalSignal
	{
		// NetworkIdentity = ServerOnly
		
		private int numberOfSpawnedPlayers;
		
		private List<int> connectedPlayers = new List<int>();
		
		private static bool hasBeenProvided;

		public List<int> ConnectedPlayers => connectedPlayers;
		public int NumberOfConnectedClients { get; set; }
		
		[Server]
		public override void OnStartServer()
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

		[Server]
		public void AddActivePlayer(int playerId)
		{
			numberOfSpawnedPlayers++;
			connectedPlayers.Add(playerId);
			print("NumberOfSpawnedPlayers = " + numberOfSpawnedPlayers);
			if (numberOfSpawnedPlayers == NumberOfConnectedClients)
			{
				print("Spawned PlayerIds:");
				foreach (var id in connectedPlayers)
				{
					print(id);
				}
				print("Mediator: All players connected to game!");
				SendGlobal(GlobalEvent.ALL_PLAYERS_CONNECTED_TO_GAME);
			}
		}

		[Server]
		public void EndOfGame()
		{
			SendGlobal(GlobalEvent.END_GAMESTATE);
		}

		public void SendGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			GlobalMediator.Instance.ReceiveGlobal(eventState, globalSignalData);
		}
	}
}