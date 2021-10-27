using System.Collections;
using Cinemachine;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Mirror;
using Player;
using TMPro;
using UI;
using UnityEngine;

namespace Managers
{
	public class HudManager : NetworkBehaviour, ISendGlobalSignal, IReceiveGlobalSignal
	{
		// NetworkIdentity = !ServerOnly
		
		[Header("REFERENCES:")]
		[SerializeField] private TMP_Text[] scoreTexts;
		[SerializeField] private GameObject newLeaderText;
		[SerializeField] private TMP_Text killedByText;
		[SerializeField] private TMP_Text respawnText;
		[SerializeField] private GameObject endScreen;
		[SerializeField] private TMP_Text winnerText;
		[SerializeField] private GameObject rematchButton;
		
		[Header("TEMP:")]
		[SerializeField] private CinemachineVirtualCamera zoomInCamera;
		[SerializeField] private CinemachineVirtualCamera flagTargetCamera;
		
		private IndicatorController indicatorController;
		private ResultsController resultsController;

		private void Awake()
		{
			indicatorController = GetComponent<IndicatorController>();
			resultsController = GetComponent<ResultsController>();
		}

		[Server]
		public override void OnStartServer()
		{
			print("HudManager provided to ServiceLocator");
			ServiceLocator.ProvideHudManager(this);
			
			GlobalMediator.Instance.Subscribe(this);
		}

		[Server]
		public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			switch (eventState)
			{
				case GlobalEvent.END_GAMESTATE:
					rematchButton.SetActive(true);
					break;
			}
		}

		[Server]
		public void UpdateFlagIndicatorTarget(bool flagHasBeenTaken, GameObject player)
		{
			if (flagHasBeenTaken)
			{
				RpcUpdateFlagIndicatorTarget(flagHasBeenTaken: true, player);
			}
			else
			{
				RpcUpdateFlagIndicatorTarget(flagHasBeenTaken: false, null);
			}
		}
		
		[ClientRpc]
		private void RpcUpdateFlagIndicatorTarget(bool flagHasBeenTaken, GameObject player)
		{
			if (flagHasBeenTaken)
			{
				indicatorController.SetTarget(targetIsAPlayer: true, player);
			}
			else
			{
				indicatorController.SetTarget(targetIsAPlayer: false);
			}
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

		[TargetRpc]
		public void TargetActivateDeathTexts(NetworkConnection conn, int attackerId)
		{
			GameObject target = conn.identity.gameObject;
			StartCoroutine(DeathTextsRoutine(target, attackerId));
		}

		[Client]
		private IEnumerator DeathTextsRoutine(GameObject target, int attackerId)
		{
			zoomInCamera.gameObject.SetActive(true);
			zoomInCamera.Follow = target.transform;
			yield return new WaitForSeconds(0.5f);
			killedByText.gameObject.SetActive(true);
			killedByText.text = "KILLED BY\n" 
			                    + $"Player_{attackerId}!";
			yield return new WaitForSeconds(1.5f);
			killedByText.gameObject.SetActive(false);
			zoomInCamera.gameObject.SetActive(false);
			flagTargetCamera.gameObject.SetActive(true);
			flagTargetCamera.Follow = indicatorController.Target.transform;
			// SendGlobal(GlobalEvent.SET_FOLLOW_TARGET, new GameObjectData(indicatorController.Target));
			yield return new WaitForSeconds(0.5f);
			respawnText.gameObject.SetActive(true);
			respawnText.text = "Respawning in... 3!";
			yield return new WaitForSeconds(1f);
			respawnText.text = "Respawning in... 2!";
			yield return new WaitForSeconds(1f);
			respawnText.text = "Respawning in... 1!";
			yield return new WaitForSeconds(0.5f);
			respawnText.gameObject.SetActive(false);
			flagTargetCamera.gameObject.SetActive(false);
			// SendGlobal(GlobalEvent.SET_FOLLOW_TARGET, new GameObjectData(target));
			yield return new WaitForSeconds(0.5f);
			target.GetComponent<PlayerController>().CmdRespawnPlayer();
			respawnText.gameObject.SetActive(true);
			respawnText.text = "V E N G E A N C E ! ! !";
			yield return new WaitForSeconds(2f);
			respawnText.gameObject.SetActive(false);
		}

		[ClientRpc]
		public void RpcActivateEndScreenAndSetWinner(string winningPlayer)
		{
			indicatorController.enabled = false;
			endScreen.SetActive(true);
			winnerText.text = $"{winningPlayer} IS THE WINNER!";
		}

		[ClientRpc]
		public void RpcCreatePlayerResult(int index, string player, int score)
		{
			resultsController.CreatePlayerResult(index, player, score);
		}

		[ServerCallback]
		private void OnDestroy()
		{
			// print("HudManager OnDestroy");
			GlobalMediator.Instance.UnSubscribe(this);
			ServiceLocator.ProvideHudManager(null);
		}
		
		public void SendGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			GlobalMediator.Instance.ReceiveGlobal(eventState, globalSignalData);
		}
	}
}