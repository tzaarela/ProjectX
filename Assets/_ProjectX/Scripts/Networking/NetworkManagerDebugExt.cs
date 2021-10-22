using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using Managers;

namespace Networking
{
	public class NetworkManagerDebugExt : NetworkRoomManagerExt
	{

		[SerializeField] GameObject debugPlayer;

		public override void Start()
		{
			base.Start();
			StartHost();
			ServiceLocator.RoundManager.NumberOfConnectedClients = 1;
			NetworkServer.SetClientReady(NetworkServer.localConnection);
			NetworkServer.ReplacePlayerForConnection(NetworkServer.localConnection, debugPlayer, true);
		}

		public override void OnServerAddPlayer(NetworkConnection conn)
		{
			//We need this to override the default behaviour
		}
	}
}