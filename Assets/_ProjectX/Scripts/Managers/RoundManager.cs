using System.Collections.Generic;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Mirror;
using UnityEngine;

namespace Managers
{
	public class RoundManager : MonoBehaviour, ISendGlobalSignal
	{
		private List<int> connectedPlayers = new List<int>();
		
		private static bool hasBeenProvided;

		public List<int> ConnectedPlayers => connectedPlayers;
		public int NumberOfConnectedClients { get; set; }
		
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

		[Server]
		public void AddActivePlayer(int playerId)
		{
			connectedPlayers.Add(playerId);
			print("NumberOfSpawnedPlayers = " + connectedPlayers.Count);
			if (connectedPlayers.Count == NumberOfConnectedClients)
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
			connectedPlayers.Clear();
		}

		[Server]
		public void SendGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			GlobalMediator.Instance.ReceiveGlobal(eventState, globalSignalData);
		}
	}
}