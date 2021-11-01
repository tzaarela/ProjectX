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
using UnityEngine.UI;

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
		[SerializeField] private Texture[] powerUpTextures;
		[SerializeField] private RawImage powerUpImage;
		[SerializeField] private TMP_Text ammoText;
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
			
			print("HudManager provided to ServiceLocator");
			ServiceLocator.ProvideHudManager(this);
		}

		[Server]
		public override void OnStartServer()
		{
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

		[Client]
		public void ActivatePowerupUi(int powerIndex, int startingAmmo)
		{
			powerUpImage.gameObject.SetActive(true);
			powerUpImage.texture = powerUpTextures[powerIndex];
			ammoText.text = startingAmmo.ToString();
		}
		
		[Client]
		public void DeactivatePowerupUi()
		{
			powerUpImage.gameObject.SetActive(false);
		}

		[TargetRpc]
		public void TargetUpdateAmmoUi(NetworkConnection conn, int currentAmmo)
		{
			ammoText.text = currentAmmo.ToString();
		}

		[TargetRpc]
		public void TargetActivateDeathTexts(NetworkConnection conn, string playerName)
		{
			GameObject target = conn.identity.gameObject;
			StartCoroutine(DeathTextsRoutine(target, playerName));
		}

		[Client]
		private IEnumerator DeathTextsRoutine(GameObject target, string attacker)
		{
			zoomInCamera.gameObject.SetActive(true);
			zoomInCamera.Follow = target.transform;
			yield return new WaitForSeconds(0.5f);
			killedByText.gameObject.SetActive(true);
			killedByText.text = "KILLED BY\n" 
			                    + $"{attacker}!";
			yield return new WaitForSeconds(2f);
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
			indicatorController.OffScreenIndicator.SetActive(false);
			killedByText.gameObject.SetActive(false);
			respawnText.gameObject.SetActive(false);
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
			GlobalMediator.Instance.UnSubscribe(this);
		}
		
		public void SendGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			GlobalMediator.Instance.ReceiveGlobal(eventState, globalSignalData);
		}
	}
}