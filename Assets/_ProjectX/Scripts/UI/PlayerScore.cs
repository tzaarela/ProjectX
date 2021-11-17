using TMPro;
using UnityEngine;

namespace UI
{
	public class PlayerScore : MonoBehaviour
	{
		[SerializeField] private TMP_Text nameText;
		[SerializeField] private TMP_Text scoreText;

		public void UpdatePlayerScore(string player, int score)
		{
			nameText.text = player;
			scoreText.text = score.ToString();
		}

		public void UpdatePlayerScore(string player)
		{
			nameText.text = player;
		}

		public void UpdatePlayerScore(int score)
		{
			scoreText.text = score.ToString();
		}
	}
}