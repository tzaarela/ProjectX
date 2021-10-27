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
		private List<string> connectedPlayers = new List<string>();
		
		private static bool hasBeenProvided;

		public List<string> ConnectedPlayers => connectedPlayers;
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
		public void AddActivePlayer(string playerName)
		{
			connectedPlayers.Add(playerName);
			print("NumberOfSpawnedPlayers = " + connectedPlayers.Count);
			if (connectedPlayers.Count == NumberOfConnectedClients)
			{
				print("Spawned PlayerIds:");
				foreach (var name in connectedPlayers)
				{
					print(name);
				}
				print("Mediator: All players connected to game!");
				SendGlobal(GlobalEvent.ALL_PLAYERS_CONNECTED_TO_GAME);
			}
		}

		[Server]
		public void EndOfGame()
		{
			connectedPlayers.Clear();
			SendGlobal(GlobalEvent.END_GAMESTATE);
		}

		[Server]
		public void SendGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			GlobalMediator.Instance.ReceiveGlobal(eventState, globalSignalData);
		}
	}
}