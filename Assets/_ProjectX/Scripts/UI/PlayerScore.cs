using Data.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class PlayerScore : MonoBehaviour
	{
		[SerializeField] private SO_CarMaterials carMaterials;
		[SerializeField] private TMP_Text nameText;
		[SerializeField] private TMP_Text scoreText;
		[SerializeField] private Image nameBgImage;
		[SerializeField] private Image scoreBgImage;
		
		public void UpdatePlayerScore(string player, int score, int matIndex)
		{
			nameBgImage.color = carMaterials.GetColor(matIndex);
			scoreBgImage.color = carMaterials.GetColor(matIndex);
			
			nameText.color = carMaterials.GetSecondaryColor(matIndex);
			scoreText.color = carMaterials.GetSecondaryColor(matIndex);
			
			nameText.text = player;
			scoreText.text = score.ToString();
		}

		public void UpdatePlayerScore(string player, int matIndex)
		{
			nameBgImage.color = carMaterials.GetColor(matIndex);
			scoreBgImage.color = carMaterials.GetColor(matIndex);
			
			nameText.color = carMaterials.GetSecondaryColor(matIndex);
			scoreText.color = carMaterials.GetSecondaryColor(matIndex);
			
			nameText.text = player;
		}

		public void UpdatePlayerScore(int score)
		{
			scoreText.text = score.ToString();
		}
	}
}