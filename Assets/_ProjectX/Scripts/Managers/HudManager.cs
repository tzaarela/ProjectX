using Mirror;
using TMPro;
using UnityEngine;

namespace Managers
{
	public class HudManager : NetworkBehaviour
	{
		// NetworkIdentity = !ServerOnly
		
		[Header("REFERENCES:")]
		[SerializeField] private TMP_Text[] scoreTexts;
		[SerializeField] private GameObject newLeaderText;
		
		[Server]
		public override void OnStartServer()
		{
			print("HudManager provided to ServiceLocator");
			ServiceLocator.ProvideHudManager(this);
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
		
		[ServerCallback]
		private void OnDestroy()
		{
			if (!isServer)
				return;
			
			ServiceLocator.ProvideHudManager(null);
		}
	}
}