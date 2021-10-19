using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Managers
{
	public class ScoreManager : NetworkBehaviour, IReceiveGlobalSignal
	{
		[SerializeField] private TMP_Text[] scoreTexts;
		[SerializeField] private GameObject newLeaderText;
		[SerializeField] private float scoreRate = 0.2f;
		[SerializeField] private int scoreToAdd = 2;

		public List<int> playerIds;
		public Dictionary<string, int> playerScores;

		private Coroutine scoreCounterRoutine;

		private string player1;
		private string currentLeader;

		[Server]
		public override void OnStartServer()
		{
			if (!isServer)
				return;

			GlobalMediator.Instance.Subscribe(this);
		}
		
		[Server]
		public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			if (eventState == GlobalEvent.ALL_PLAYERS_CONNECTED_TO_GAME)
			{
				print("ScoreManager Started!");
				playerIds= new List<int>(ServiceLocator.RoundManager.ConnectedPlayers);
				playerScores = new Dictionary<string, int>();
				foreach (int id in playerIds)
				{
					playerScores.Add("Player_" + id, 0);
					player1 = "Player_" + id;
					currentLeader = player1;
				}
				//TEMP:
				playerScores.Add("PlayerTemp_1", 0);
				playerScores.Add("PlayerTemp_2", 0);
				SortPlayerScores(playerScores);
			}
		}
		
		private void Update()
		{
			if (Keyboard.current.digit1Key.wasPressedThisFrame)
			{
				UpdateScore(player1);
			}
			if (Keyboard.current.digit2Key.wasPressedThisFrame)
			{
				UpdateScore("PlayerTemp_1");
			}
			if (Keyboard.current.digit3Key.wasPressedThisFrame)
			{
				UpdateScore("PlayerTemp_2");
			}
			if (Keyboard.current.digit4Key.wasPressedThisFrame)
			{
				StopScoreCounter();
			}
		}
		
		private void UpdateScore(string playerId)
		{
			StopScoreCounter();
			scoreCounterRoutine = StartCoroutine(ScoringRoutine(playerId));
		}

		private IEnumerator ScoringRoutine(string playerId)
		{
			float time = 0;
			while (time < scoreRate)
			{
				time += Time.deltaTime;				
				yield return null;
			}

			playerScores[playerId] += scoreToAdd;
			SortPlayerScores(playerScores);

			scoreCounterRoutine = StartCoroutine(ScoringRoutine(playerId));
		}
		
		private void StopScoreCounter()
		{
			if (scoreCounterRoutine == null)
				return;
			
			StopCoroutine(scoreCounterRoutine);
		}
		
		private void SortPlayerScores(Dictionary<string, int> scores)
		{
			playerScores = scores.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
			
			int index = 0; 
			foreach(KeyValuePair<string, int> kvp in playerScores)
			{
				if (index <= 2)
				{
					// if (index == 0 && !kvp.Key.Equals(currentLeader))
					// {
					// 	currentLeader = kvp.Key;
					// 	newLeaderText.SetActive(true);
					// }
					RpcUpdateHudScore(index, kvp.Key, kvp.Value);
					// print("Index: " + index);
					// print(kvp.Key + ": " + kvp.Value);
					index++;
				}
			}
		}

		// Should be from Command? Authority??
		[ClientRpc]
		private void RpcUpdateHudScore(int index, string kvpKey, int kvpValue)
		{
			scoreTexts[index].text = kvpKey + ":\n" +
			                         kvpValue;
			
			if (index == 0 && !kvpKey.Equals(currentLeader))
			{
				currentLeader = kvpKey;
				newLeaderText.SetActive(true);
			}
		}
	}
}