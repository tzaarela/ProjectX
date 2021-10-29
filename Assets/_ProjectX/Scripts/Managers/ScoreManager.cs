using System;
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
		[SerializeField] private float scoreRate = 0.5f;
		[SerializeField] private int scoreToAdd = 5;
		[SerializeField] private int scoreToWin = 100;
		
		private Dictionary<string, int> playerScores;

		private Coroutine scoreCounterRoutine;
		
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
			switch (eventState)
			{
				case GlobalEvent.ALL_PLAYERS_CONNECTED_TO_GAME:
				{
					List<string> playerNames = ServiceLocator.RoundManager.ConnectedPlayers;
					playerScores = new Dictionary<string, int>();
					foreach (string name in playerNames)
					{
						playerScores.Add(name, 0);
					}
				
					//TEMP:
					playerScores.Add("PlayerTemp_1", 10);
					playerScores.Add("PlayerTemp_2", 40);
					playerScores.Add("PlayerTemp_3", 0);
					playerScores.Add("PlayerTemp_4", 20);
				
					playerScores = SortedByAscendingKey(playerScores);
					InitScores();
					break;
				}
				case GlobalEvent.END_GAMESTATE:
					StopAllCoroutines();
					ActivateEndScreenWithFinalResults();
					break;
			}
		}

		[Server]
		public void InitializeScoring(string playerName)
		{
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
		private IEnumerator ScoringRoutine(string player)
		{
			UpdateScores(player);
			
			yield return new WaitForSeconds(scoreRate);
			
			scoreCounterRoutine = StartCoroutine(ScoringRoutine(player));
		}

		[Server]
		private void UpdateScores(string player)
		{
			playerScores[player] += scoreToAdd;

			if (playerScores[player] >= scoreToWin)
			{
				ServiceLocator.RoundManager.EndOfGame();
				return;
			}
			
			UpdateScores();
		}

		[Server]
		private void UpdateScores()
		{
			playerScores = SortedByDescendingValue(playerScores);
			
			int index = 0;
			foreach (KeyValuePair<string, int> kvp in playerScores.Where(kvp => index <= 2))
			{
				if (index == 0 && !string.Equals(kvp.Key, currentLeader, StringComparison.OrdinalIgnoreCase))
				{
					currentLeader = kvp.Key;
					ServiceLocator.HudManager.RpcActivateNewLeaderText();
				}

				ServiceLocator.HudManager.RpcUpdateScore(index, kvp.Key, kvp.Value);
				index++;
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
		
		[Server]
		private void ActivateEndScreenWithFinalResults()
		{
			playerScores = SortedByDescendingValue(playerScores);
			
			ServiceLocator.HudManager.RpcActivateEndScreenAndSetWinner(playerScores.ElementAt(0).Key);
			
			int index = 0;
			foreach (KeyValuePair<string, int> kvp in playerScores)
			{
				ServiceLocator.HudManager.RpcCreatePlayerResult(index, kvp.Key, kvp.Value);
				index++;
			}
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
		
		[ServerCallback]
		private void OnDestroy()
		{
			// print("ScoreManager OnDestroy");
			GlobalMediator.Instance.UnSubscribe(this);
			ServiceLocator.ProvideScoreManager(null);
		}
	}
}