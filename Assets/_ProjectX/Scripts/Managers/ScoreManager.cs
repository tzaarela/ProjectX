using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Mirror;
using UnityEngine;

namespace Managers
{
	public class ScoreManager : NetworkBehaviour, IReceiveGlobalSignal
	{
		// NetworkIdentity = ServerOnly

		[Header("SETTINGS:")]
		[SerializeField] private float scoreRate = 0.2f;
		[SerializeField] private int scoreToAdd = 2;
		
		private Dictionary<string, int> playerScores;

		private Coroutine scoreCounterRoutine;

		private string playerPrefix = "Player_";
		private string currentLeader = "JohnDoe";

		[Server]
		public override void OnStartServer()
		{
			print("ScoreManager provided to ServiceLocator");
			ServiceLocator.ProvideScoreManager(this);
			
			GlobalMediator.Instance.Subscribe(this);
		}
		
		[Server]
		public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			if (eventState == GlobalEvent.ALL_PLAYERS_CONNECTED_TO_GAME)
			{
				print("ScoreManager Started!");
				
				List<int> playerIds = new List<int>(ServiceLocator.RoundManager.ConnectedPlayers);
				playerScores = new Dictionary<string, int>();
				foreach (int id in playerIds)
				{
					playerScores.Add(playerPrefix + id, 0);
				}
				
				//TEMP:
				playerScores.Add("PlayerTemp_1", 0);
				playerScores.Add("PlayerTemp_2", 0);
				
				playerScores = SortedByAscendingKey(playerScores);
				InitScores();
			}
		}

		[Server]
		public void InitializeScoring(int playerId)
		{
			string playerName = playerPrefix + playerId;
			print("Scoring Initialized - " + playerName);
			StopScoreCounter();
			scoreCounterRoutine = StartCoroutine(ScoringRoutine(playerName));
		}

		[Server]
		public void StopScoreCounter()
		{
			if (scoreCounterRoutine == null)
				return;
			
			StopCoroutine(scoreCounterRoutine);
		}
		
		[Server]
		private IEnumerator ScoringRoutine(string playerId)
		{
			UpdateScores(playerId);
			
			yield return new WaitForSeconds(scoreRate);
			
			scoreCounterRoutine = StartCoroutine(ScoringRoutine(playerId));
		}

		[Server]
		private void UpdateScores(string playerId)
		{
			playerScores[playerId] += scoreToAdd;
			playerScores = SortedByDescendingValue(playerScores);
			
			UpdateScores();
		}

		[Server]
		private Dictionary<string, int> SortedByDescendingValue(Dictionary<string, int> scores)
		{
			return scores.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
		}
		
		[Server]
		private Dictionary<string, int> SortedByAscendingKey(Dictionary<string, int> scores)
		{
			return scores.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
		}

		[Server]
		private void UpdateScores()
		{
			int index = 0;
			foreach (KeyValuePair<string, int> kvp in playerScores.Where(kvp => index <= 2))
			{
				ServiceLocator.HudManager.RpcUpdateScore(index, kvp.Key, kvp.Value);
				index++;
				
				if (index == 0 && !kvp.Key.Equals(currentLeader))
				{
					currentLeader = kvp.Key;
					ServiceLocator.HudManager.RpcActivateNewLeaderText();
				}
			}
		}
		
		[Server]
		private void InitScores()
		{
			int index = 0;
			foreach (KeyValuePair<string, int> kvp in playerScores.Where(kvp => index <= 2))
			{
				ServiceLocator.HudManager.RpcUpdateScore(index, kvp.Key, kvp.Value);
				index++;
			}
		}
	}
}