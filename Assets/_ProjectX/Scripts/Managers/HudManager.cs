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
using DG.Tweening;
using System;

namespace Managers
{
	public class HudManager : NetworkBehaviour, ISendGlobalSignal, IReceiveGlobalSignal
	{
		// NetworkIdentity = !ServerOnly

		[Header("SETTINGS:")]
		[SerializeField] private float countdownScalePunchMultiplier = 0.5f;
		[SerializeField] private float countdownScalePunchDuration = 0.25f;
		[SerializeField] private int countdownScalePunchVibrato = 4;
		
		[SerializeField] private float powerupScalePunchMultiplier = 1.1f;
		[SerializeField] private float powerupScalePunchDuration = 0.5f;
		[SerializeField] private int powerupScalePunchVibrato = 1;
		[SerializeField] private float powerupScalePunchElasticity = 1;

		[Header("REFERENCES:")]
		[SerializeField] private PlayerScore[] playerScores;
		[SerializeField] private GameObject newLeaderText;
		[SerializeField] private TMP_Text countdownText;
		[SerializeField] private TMP_Text flagText;
		[SerializeField] private TMP_Text killText;
		[SerializeField] private TMP_Text respawnText;
		[SerializeField] private Texture[] powerUpTextures;
		[SerializeField] private RawImage powerUpImage;
		[SerializeField] private TMP_Text ammoText;
		[SerializeField] private Image boostBar;
		[SerializeField] private GameObject endScreen;
		[SerializeField] private TMP_Text winnerText;
		[SerializeField] private GameObject rematchButton;

		[Header("EXT. REFERENCES:")]
		[SerializeField] private CinemachineVirtualCamera zoomInCamera;
		[SerializeField] private CinemachineVirtualCamera flagTargetCamera;
		[SerializeField] private CinemachineVirtualCamera flagIntroCamera;

		private IndicatorController indicatorController;
		private ResultsController resultsController;
		private Tweener powerupScalePunchTweener;
		private Tweener countdownScalePunchTweener;

		private void Awake()
		{
			ServiceLocator.ProvideHudManager(this);
		}

		[Server]
		public override void OnStartServer()
		{
			GlobalMediator.Instance.Subscribe(this);
		}

		public override void OnStartClient()
		{
			indicatorController = GetComponent<IndicatorController>();
			resultsController = GetComponent<ResultsController>();
			boostBar.fillAmount = 1f;
			flagText.text = "";
			killText.text = "";

			NetworkClient.ReplaceHandler<ScoreMessage>(UpdateScore);

			base.OnStartClient();
		}



		[Client]
		private void UpdateScore(ScoreMessage scoreMessage)
		{
			switch (scoreMessage.scoreType)
			{
				case ScoreType.Init:
					playerScores[scoreMessage.index].gameObject.SetActive(true);
					playerScores[scoreMessage.index].UpdatePlayerScore(scoreMessage.player, scoreMessage.score, scoreMessage.matIndex);
					break;
				case ScoreType.UpdateScore:
					playerScores[scoreMessage.index].UpdatePlayerScore(scoreMessage.player, scoreMessage.score, scoreMessage.matIndex);
					break;
				case ScoreType.UpdatePlayerScore:
					playerScores[scoreMessage.index].UpdatePlayerScore(scoreMessage.player, scoreMessage.matIndex);
					StartCoroutine(ScoreCounterRoutine(scoreMessage.index, scoreMessage.previousScore, scoreMessage.score, scoreMessage.scoreRate));
					break;
			}


		}

		[Server]
		public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			switch (eventState)
			{ 
				case GlobalEvent.ALL_PLAYERS_CONNECTED_TO_GAME:

					RpcStartCountDown();
					break;
					
				case GlobalEvent.FLAG_TAKEN:

					if (globalSignalData is GameObjectData playerTakingFlag)
					{
						string playerName = playerTakingFlag.gameObject.GetComponent<PlayerController>().playerName;
						string newStatusText = $"{playerName} has the flag";
						RpcUpdateFlagStatusText(newStatusText);
						RpcUpdateFlagIndicatorTarget(true, playerTakingFlag.gameObject);
					}
					break;
                
				case GlobalEvent.FLAG_DROPPED:
                    
					if (globalSignalData is GameObjectData playerDroppingFlag)
					{
						string playerName = playerDroppingFlag.gameObject.GetComponent<PlayerController>().playerName;
						string newStatusText = $"{playerName} dropped the flag";
						RpcUpdateFlagStatusText(newStatusText);
						RpcUpdateFlagIndicatorTarget(flagHasBeenTaken: false, null);
					}
					break;
				
				// case GlobalEvent.END_GAMESTATE:
				// 	rematchButton.SetActive(true);
				// 	break;
			}
		}

		[ClientRpc]
		private void RpcStartCountDown()
		{
			StartCoroutine(CountDownRoutine());
		}

		[Client]
		private IEnumerator CountDownRoutine()
		{
			FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/FlagFlapping", Camera.main.transform.position);
			yield return new WaitForSeconds(1.5f);
			flagIntroCamera.gameObject.SetActive(false);
			flagTargetCamera.gameObject.SetActive(true);
			yield return new WaitForSeconds(0.75f);
			FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Countdown", Camera.main.transform.position);
			yield return new WaitForSeconds(0.05f);
			countdownText.gameObject.SetActive(true);
			CountDownScalePunchTween();
			countdownText.text = "3";
			yield return new WaitForSeconds(0.5f);
			countdownText.text = "";
			yield return new WaitForSeconds(0.5f);
			CountDownScalePunchTween();
			countdownText.text = "2";
			yield return new WaitForSeconds(0.5f);
			countdownText.text = "";
			yield return new WaitForSeconds(0.5f);
			CountDownScalePunchTween();
			countdownText.text = "1";
			yield return new WaitForSeconds(0.5f);
			countdownText.text = "";
			yield return new WaitForSeconds(0.2f);
			flagTargetCamera.gameObject.SetActive(false);
			yield return new WaitForSeconds(0.3f);
			SendGlobal(GlobalEvent.END_OF_COUNTDOWN);
		}
		
		[Client]
		private void CountDownScalePunchTween()
		{
			if (!countdownScalePunchTweener.IsActive())
			{
				countdownScalePunchTweener = countdownText.rectTransform.DOPunchScale(Vector3.one * countdownScalePunchMultiplier,
																						countdownScalePunchDuration, countdownScalePunchVibrato, 1f);
			}
		}
		
		[ClientRpc]
		private void RpcUpdateFlagStatusText(string statusText)
		{
			flagText.text = statusText;
			flagText.gameObject.SetActive(false);
			flagText.gameObject.SetActive(true);
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

		[Client]
		private IEnumerator ScoreCounterRoutine(int index, int previousScore, int newScore, float scoreRate)
		{
			int scoreToDisplay = previousScore;
			int scoreDifference = newScore - previousScore;
			
			while (scoreToDisplay < newScore)
			{
				scoreToDisplay++;
				playerScores[index].UpdatePlayerScore(scoreToDisplay);
				yield return new WaitForSeconds(scoreRate / scoreDifference);
			}
		}
		
		[ClientRpc]
		public void RpcActivateNewLeaderText()
		{
			FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Fanfare", Camera.main.transform.position);
			newLeaderText.SetActive(true);
		}

		[Client]
		public void ActivatePowerupUi(int powerIndex, int startingAmmo)
		{
			powerUpImage.gameObject.SetActive(true);
			powerUpImage.texture = powerUpTextures[powerIndex];
			ammoText.text = startingAmmo.ToString();

			PowerupScalePunchTween();
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
			PowerupScalePunchTween();
		}

		[Client]
		private void PowerupScalePunchTween()
		{
			if (!powerupScalePunchTweener.IsActive())
			{
				powerupScalePunchTweener = ammoText.rectTransform.DOPunchScale(Vector3.one * powerupScalePunchMultiplier, powerupScalePunchDuration,
					powerupScalePunchVibrato, powerupScalePunchElasticity);
			}
		}

		[TargetRpc]
		public void TargetUpdateBoostBar(NetworkConnection conn, float percent)
		{
			boostBar.fillAmount = percent;
		}
		
		[TargetRpc]
		public void TargetActivateKillText(NetworkConnection conn, string playerName)
		{
			killText.text = $"You killed {playerName}";
			killText.gameObject.SetActive(false);
			killText.gameObject.SetActive(true);
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
			yield return new WaitForSeconds(0.5f);
			killText.text = $"Killed by {attacker}";
			killText.gameObject.SetActive(false);
			killText.gameObject.SetActive(true);
			yield return new WaitForSeconds(2f);
			zoomInCamera.gameObject.SetActive(false);
			flagTargetCamera.gameObject.SetActive(true);
			yield return new WaitForSeconds(0.5f);
			respawnText.gameObject.SetActive(true);
			respawnText.text = "Respawning in... 3";
			yield return new WaitForSeconds(1f);
			respawnText.text = "Respawning in... 2";
			yield return new WaitForSeconds(1f);
			respawnText.text = "Respawning in... 1";
			yield return new WaitForSeconds(0.5f);
			respawnText.gameObject.SetActive(false);
			yield return new WaitForSeconds(0.5f);
			flagTargetCamera.gameObject.SetActive(false);
			target.GetComponent<PlayerController>().CmdRespawnPlayer();
			// respawnText.gameObject.SetActive(true);
			// respawnText.text = "V E N G E A N C E ! ! !";
			// yield return new WaitForSeconds(2f);
			// respawnText.gameObject.SetActive(false);
		}

		[ClientRpc]
		public void RpcActivateEndScreenAndSetWinner(string winningPlayer)
		{
			indicatorController.enabled = false;
			killText.gameObject.SetActive(false);
			respawnText.gameObject.SetActive(false);
			endScreen.SetActive(true);
			winnerText.text = $"{winningPlayer} is the winner!";
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