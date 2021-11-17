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

namespace Managers
{
	public class HudManager : NetworkBehaviour, ISendGlobalSignal, IReceiveGlobalSignal
	{
		// NetworkIdentity = !ServerOnly

		[Header("SETTINGS")]
		[SerializeField] private float powerupScalePunchMultiplier = 1.1f;
		[SerializeField] private float powerupScalePunchDuration = 0.5f;
		[SerializeField] private int powerupScalePunchVibrato = 1;
		[SerializeField] private float powerupScalePunchElasticity = 1;

		[Header("REFERENCES:")]
		[SerializeField] private PlayerScore[] playerScores;
		// [SerializeField] private TMP_Text[] scoreTexts;
		[SerializeField] private GameObject newLeaderText;
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

		private IndicatorController indicatorController;
		private ResultsController resultsController;
		private Tweener powerupScalePunchTweener;

		private void Awake()
		{
			indicatorController = GetComponent<IndicatorController>();
			resultsController = GetComponent<ResultsController>();
			boostBar.fillAmount = 1f;
			flagText.text = "";
			killText.text = "";

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
				
				case GlobalEvent.END_GAMESTATE:
					// rematchButton.SetActive(true);
					break;
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
		
		[ClientRpc]
		public void RpcInitScore(int index, string player, int score, int matIndex)
		{
			playerScores[index].gameObject.SetActive(true);
			playerScores[index].UpdatePlayerScore(player, score, matIndex);
		}

		[ClientRpc]
		public void RpcUpdateScore(int index, string player, int score, int matIndex)
		{
			playerScores[index].UpdatePlayerScore(player, score, matIndex);
		}

		[ClientRpc]
		public void RpcUpdateScoringPlayerScore(int index, string player, int matIndex, int score, int previousScore, float scoreRate)
		{
			playerScores[index].UpdatePlayerScore(player, matIndex);
			// scoreTexts[index].text = score.ToString();
			StartCoroutine(ScoreCounterRoutine(index, previousScore, score, scoreRate));
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
				powerupScalePunchTweener = powerUpImage.rectTransform.DOPunchScale(Vector3.one * powerupScalePunchMultiplier, powerupScalePunchDuration,
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
			flagTargetCamera.gameObject.SetActive(false);
			yield return new WaitForSeconds(0.5f);
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