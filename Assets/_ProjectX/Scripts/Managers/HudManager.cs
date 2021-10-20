using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Mirror;
using TMPro;
using UnityEngine;

namespace Managers
{
	public class HudManager : NetworkBehaviour, IReceiveGlobalSignal
	{
		// NetworkIdentity = !ServerOnly
		
		[Header("REFERENCES:")]
		[SerializeField] private TMP_Text[] scoreTexts;
		[SerializeField] private GameObject newLeaderText;
		[SerializeField] private GameObject endScreen;
		
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
		private void RpcActivateEndScreen()
		{
			endScreen.SetActive(true);
		}
		
		[Server]
		public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			if (eventState == GlobalEvent.END_GAMESTATE)
			{
				RpcActivateEndScreen();
			}
		}

		[ServerCallback]
		private void OnDestroy()
		{
			if (!isServer)
				return;
			
			ServiceLocator.ProvideHudManager(null);
		}
	}
}