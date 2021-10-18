using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using Managers;

namespace Networking
{
	public class NetworkManagerDebugExt : NetworkManager
	{

		[SerializeField] GameObject debugPlayer;


		public override void Start()
		{
			base.Start();
			StartHost();
			//NetworkServer.AddPlayerForConnection(NetworkServer.localConnection, debugPlayer);
			ServiceLocator.RoundManager.NumberOfConnectedClients = 1;
			NetworkServer.SetClientReady(NetworkServer.localConnection);
			NetworkServer.ReplacePlayerForConnection(NetworkServer.localConnection, debugPlayer, true);

			//NetworkServer.localConnection.identity.gameObject.GetComponent<PlayerController>().OnStartClient();
	}

		public override void OnServerAddPlayer(NetworkConnection conn)
		{
		}
	}
}