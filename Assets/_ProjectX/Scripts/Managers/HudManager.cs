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
		
		[ServerCallback]
		private void OnDestroy()
		{
			if (!isServer)
				return;
			
			ServiceLocator.ProvideHudManager(null);
		}
	}
}