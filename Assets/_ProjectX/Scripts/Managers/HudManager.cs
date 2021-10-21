using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Mirror;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
	public class HudManager : NetworkBehaviour, IReceiveGlobalSignal
	{
		// NetworkIdentity = !ServerOnly
		
		[Header("REFERENCES:")]
		[SerializeField] private TMP_Text[] scoreTexts;
		[SerializeField] private GameObject newLeaderText;
		[SerializeField] private GameObject endScreen;
		[SerializeField] private TMP_Text winnerText;
		[SerializeField] private GameObject rematchButton;

		[Server]
		public override void OnStartServer()
		{
			print("HudManager provided to ServiceLocator");
			ServiceLocator.ProvideHudManager(this);
			
			GlobalMediator.Instance.Subscribe(this);
		}

		[ClientRpc]
		public void RpcUpdateScore(int index, string player, int score)
		{
			scoreTexts[index].text = player + ":\n" +
			                         score;
		}

		[ClientRpc]
		public void RpcActivateNewLeaderText()
		{
			newLeaderText.SetActive(true);
		}

		[ClientRpc]
		public void RpcActivateEndScreenAndSetWinner(string winningPlayer)
		{
			endScreen.SetActive(true);
			winnerText.text = $"{winningPlayer} IS THE WINNER!";
		}

		[ClientRpc]
		public void RpcCreatePlayerResult(int index, string player, int score)
		{
			GetComponent<ResultsController>().CreatePlayerResult(index, player, score);
		}
		
		[Server]
		public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			if (eventState == GlobalEvent.END_GAMESTATE)
			{
				rematchButton.SetActive(true);
			}
		}

		[Server]
		public void ResetTimeScale()
		{
			GetComponent<TimeController>().RpcSetTimeScale(1);
		}

		public void LoadMainMenuScene()
		{
			SceneManager.LoadScene("MainMenu");
		}

		[ServerCallback]
		private void OnDestroy()
		{
			// print("HudManager OnDestroy");
			GlobalMediator.Instance.UnSubscribe(this);
			ServiceLocator.ProvideHudManager(null);
		}
	}
}