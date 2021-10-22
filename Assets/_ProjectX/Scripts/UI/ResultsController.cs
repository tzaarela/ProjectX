﻿using Mirror;
using TMPro;
using UnityEngine;

namespace UI
{
	public class ResultsController : MonoBehaviour
	{
		[SerializeField] private GameObject playerGroup;
		[SerializeField] private GameObject scoreGroup;
		[SerializeField] private GameObject resultsText;

		[SerializeField] private TMP_Text[] playerTexts;
		[SerializeField] private TMP_Text[] scoreTexts;
		
		public void CreatePlayerResult(int index, string player, int score)
		{
			// print("ResultsController Index: " + index);
			// if (index < 6)
			// {
				playerTexts[index].text = player;
				playerTexts[index].gameObject.SetActive(true);
				scoreTexts[index].text = score.ToString();
				scoreTexts[index].gameObject.SetActive(true);
			// }
			// NOT WORKING! WHY? Was called as Server, not Rpc!
			// GameObject playerText = Instantiate(resultsText, playerGroup.transform);
			// playerText.GetComponent<TMP_Text>().text = player;
			// NetworkServer.Spawn(playerText);
			// GameObject scoreText = Instantiate(resultsText, scoreGroup.transform);
			// scoreText.GetComponent<TMP_Text>().text = score.ToString();
			// scoreText.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Right;
			// NetworkServer.Spawn(scoreText);
		}
	}
}