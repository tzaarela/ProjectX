using TMPro;
using UnityEngine;

namespace UI
{
	public class TimeController : MonoBehaviour
	{
		[SerializeField] private TMP_Text timeText;
		[SerializeField] private float timeLimit = 100f;

		private float timeRemaining;

		private void Start()
		{
			timeRemaining = timeLimit;
		}

		private void Update()
		{
			if (timeRemaining > 0)
			{
				timeRemaining -= Time.deltaTime;
				timeText.text =  Mathf.CeilToInt(timeRemaining).ToString();
			}
			else
			{
				timeText.text = "Out of time!";
			}
		}
	}
}