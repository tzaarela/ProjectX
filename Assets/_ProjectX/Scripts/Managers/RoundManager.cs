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
		[Header("SETTINGS:")]
		public int roundTime = 300;
		public float scoreRate = 0.5f;
		public int scoreToAdd = 5;
		public int scoreToWin = 100;
		public int additionalScoringStartupThreshold = 100;
		public int additionalScoringMaxThreshold = 300;
		public float additionalScoreMaxMultiplier = 2;
		
		private List<string> connectedPlayers = new List<string>();
		
		private static bool hasBeenProvided;

		public List<string> ConnectedPlayers => connectedPlayers;
		public int NumberOfConnectedClients { get; set; }
		
		[Server]
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
				print("Spawned Players:");
				foreach (var name in connectedPlayers)
				{
					print(name);
				}
				print("Mediator: All players connected to game!");
				SendGlobal(GlobalEvent.ALL_PLAYERS_CONNECTED_TO_GAME, new GameObjectData(gameObject));
			}
		}

		[Server]
		public void EndOfGame()
		{
			print("---- GAME HAS ENDED ----");
			// connectedPlayers.Clear();
			SendGlobal(GlobalEvent.END_GAMESTATE);
			ServiceLocator.ProvideRoundManager(this);
			hasBeenProvided = false;
			Destroy(gameObject);
		}

		[Server]
		public void SendGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			GlobalMediator.Instance.ReceiveGlobal(eventState, globalSignalData);
		}
	}
}