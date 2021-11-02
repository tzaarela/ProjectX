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
		[SerializeField] private int additionalScoringThreshold = 50;
		[SerializeField] private float additionalScoreMaxMultiplier = 2;

		private Dictionary<string, int> playerScores;

		private Coroutine scoreCounterRoutine;
		
		private string currentLeader;
		private int currentLeaderScore;

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
					playerScores.Add("PlayerTemp_1", 0);
					playerScores.Add("PlayerTemp_2", 150);
					playerScores.Add("PlayerTemp_3", 50);
					currentLeader = "PlayerTemp_2";
					currentLeaderScore = 150;
					//
					
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
			int previousScore = playerScores[player];
			int additionalScore = CheckAdditionalScore(playerScores[player]);
			playerScores[player] += scoreToAdd + additionalScore;
			// print("ScoreAdded: " + (scoreToAdd + additionalScore));

			if (playerScores[player] >= scoreToWin)
			{
				playerScores[player] = scoreToWin;
				ServiceLocator.RoundManager.EndOfGame();
				return;
			}
			
			UpdateScores(player, previousScore);
		}

		[Server]
		private int CheckAdditionalScore(int playerScore)
		{
			int scoreDifference = currentLeaderScore - playerScore;

			if (scoreDifference < 0)
			{
				currentLeaderScore = playerScore;
			}
			else if (scoreDifference > additionalScoringThreshold)
			{
				int additionalScore = Mathf.Clamp((scoreDifference - additionalScoringThreshold) / 10, 1, (int)(scoreToAdd * additionalScoreMaxMultiplier));
				return additionalScore;
			}

			return 0;
		}

		[Server]
		private void UpdateScores(string scoringPlayer, int previousScore)
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
				
				if (string.Equals(kvp.Key, scoringPlayer, StringComparison.OrdinalIgnoreCase))
				{
					ServiceLocator.HudManager.RpcUpdateScoringPlayerScore(index, kvp.Key, kvp.Value, previousScore, scoreRate);		
				}
				else
				{
					ServiceLocator.HudManager.RpcUpdateScore(index, kvp.Key, kvp.Value);					
				}
				
				index++;
			}
		}

		[Server]
		private void InitScores()
		{
			playerScores = SortedByAscendingKey(playerScores);
			
			// TEMP:
			playerScores = SortedByDescendingValue(playerScores);
			//
			
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