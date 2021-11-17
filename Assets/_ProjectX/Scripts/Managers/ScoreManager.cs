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

		private int flagScoreEachTick;
		private float flagScoreTickRate;
		private int killScore;
		private int killWithFlagAdditionalScore;
		private int scoreToWin;
		private int additionalScoringStartupThreshold;
		private int additionalScoringMaxThreshold;
		private float additionalScoreMaxMultiplier;

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
					if (globalSignalData is GameObjectData data)
					{
						if (data.gameObject.TryGetComponent(out RoundManager roundManager))
						{
							flagScoreEachTick = roundManager.flagScoreEachTick;
							flagScoreTickRate = roundManager.flagScoreTickRate;
							killScore = roundManager.killScore;
							killWithFlagAdditionalScore = roundManager.killWithFlagAdditionalScore;
							scoreToWin = roundManager.scoreToWin;
							additionalScoringStartupThreshold = roundManager.additionalScoringStartupThreshold;
							additionalScoringMaxThreshold = roundManager.additionalScoringMaxThreshold;
							additionalScoreMaxMultiplier =roundManager.additionalScoreMaxMultiplier;
						}
						else
						{
							Debug.Log("RoundManager was not received by ScoreManager on ALL_PLAYERS_CONNECTED_TO_GAME");
						}
					}
					
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
					playerScores.Add("PlayerTemp_4", 30);
					playerScores.Add("PlayerTemp_5", 70);
					// playerScores.Add("PlayerTemp_6", 10);
					// playerScores.Add("PlayerTemp_7", 120);
					currentLeader = "PlayerTemp_2";
					currentLeaderScore = 150;
					
					
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
			
			yield return new WaitForSeconds(flagScoreTickRate);
			
			scoreCounterRoutine = StartCoroutine(ScoringRoutine(player));
		}

		[Server]
		private void UpdateScores(string player)
		{
			int previousScore = playerScores[player];
			int additionalScore = CheckAdditionalScore(playerScores[player]);
			playerScores[player] += flagScoreEachTick + additionalScore;
			// print("ScoreAdded: " + (scoreToAdd + additionalScore));
			
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
			else if (scoreDifference > additionalScoringStartupThreshold)
			{
				int additionalScore = Mathf.Clamp((scoreDifference - additionalScoringStartupThreshold) /
				                                  ((additionalScoringMaxThreshold - additionalScoringStartupThreshold) / flagScoreEachTick)
													,1, (int)(flagScoreEachTick * additionalScoreMaxMultiplier));
				return additionalScore;
			}

			return 0;
		}

		[Server]
		private void UpdateScores(string scoringPlayer, int previousScore)
		{
			if (playerScores[scoringPlayer] >= scoreToWin)
			{
				playerScores[scoringPlayer] = scoreToWin;
				ServiceLocator.RoundManager.EndOfGame();
				return;
			}
			
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
					ServiceLocator.HudManager.RpcUpdateScoringPlayerScore(index, kvp.Key, kvp.Value, previousScore, flagScoreTickRate);		
				}
				else
				{
					ServiceLocator.HudManager.RpcUpdateScore(index, kvp.Key, kvp.Value);					
				}
				
				index++;
			}
		}

		[Server]
		public void AddKillScore(string player, bool hasFlag)
		{
			int previousScore = playerScores[player];

			playerScores[player] += !hasFlag ? killScore : killScore + killWithFlagAdditionalScore;
			// print("KillScore Added: " + (playerScores[player] - previousScore));

			UpdateScores(player, previousScore);
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