using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI
{
	public class ScoreController : MonoBehaviour
	{
		[SerializeField] private TMP_Text scorePlayer1;
		[SerializeField] private TMP_Text scorePlayer2;
		[SerializeField] private TMP_Text scorePlayer3;
		[SerializeField] private float scoreRate = 0.2f;
		[SerializeField] private int scoreToAdd = 2;

		private int score1 = 0;
		private int score2 = 0;
		private int score3 = 0;

		private Coroutine scoreCounterRoutine;
		
		private void Update()
		{
			if (Keyboard.current.digit1Key.wasPressedThisFrame)
			{
				UpdateScore(1);
			}
			if (Keyboard.current.digit2Key.wasPressedThisFrame)
			{
				UpdateScore(2);
			}
			if (Keyboard.current.digit3Key.wasPressedThisFrame)
			{
				UpdateScore(3);
			}
			if (Keyboard.current.digit4Key.wasPressedThisFrame)
			{
				StopScoreCounter();
			}
		}

		private void UpdateScore(int playerNumber)
		{
			StopScoreCounter();
			switch (playerNumber)
			{
				case 1:
					scoreCounterRoutine = StartCoroutine(InitScoreCounterRoutine(1, scorePlayer1));
					break;
				case 2:
					scoreCounterRoutine = StartCoroutine(InitScoreCounterRoutine(2, scorePlayer2));
					break;
				case 3:
					scoreCounterRoutine = StartCoroutine(InitScoreCounterRoutine(3, scorePlayer3));
					break;
				default:
					Debug.LogError("PlayerNumber out of range in ScoreController/UpdateScore()");
					break;
			}
		}

		private IEnumerator InitScoreCounterRoutine(int playerNumber, TMP_Text tmpText)
		{
			float time = 0;
			while (time < scoreRate)
			{
				time += Time.deltaTime;				
				yield return null;
			}

			switch (playerNumber)
			{
				case 1:
					score1 += scoreToAdd;
					tmpText.text = score1.ToString();
					break;
				case 2:
					score2 += scoreToAdd;
					tmpText.text = score2.ToString();
					break;
				case 3:
					score3 += scoreToAdd;
					tmpText.text = score3.ToString();
					break;
			}
			scoreCounterRoutine = StartCoroutine(InitScoreCounterRoutine(playerNumber, tmpText));
		}

		private void StopScoreCounter()
		{
			if (scoreCounterRoutine == null)
				return;
			
			StopCoroutine(scoreCounterRoutine);
		}
	}
}