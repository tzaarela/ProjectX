using Mirror;
using TMPro;
using UnityEngine;

namespace UI
{
	public class ResultsController : MonoBehaviour
	{
		[SerializeField] private TMP_Text[] playerTexts;
		[SerializeField] private TMP_Text[] scoreTexts;
		
		public void CreatePlayerResult(int index, string player, int score)
		{
			playerTexts[index].text = player;
			playerTexts[index].gameObject.SetActive(true);
			scoreTexts[index].text = score.ToString();
			scoreTexts[index].gameObject.SetActive(true);
		}
	}
}