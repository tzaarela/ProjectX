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
			if (!isServer)
				return;
			
			print("HudManager provided to ServiceLocator");
			ServiceLocator.ProvideHudManager(this);
		}
		
		[Client]
		public void UpdateTopThreeScore(int index, string player, int score)
		{
			scoreTexts[index].text = player + ":\n" +
			                         score;
		}

		[Client]
		public void ActivateNewLeaderText()
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