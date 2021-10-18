using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerDebugExt : NetworkManager
{

	[SerializeField] GameObject debugPlayer;


	public override void Start()
	{
		base.Start();
		StartHost();
		//NetworkServer.AddPlayerForConnection(NetworkServer.localConnection, debugPlayer);
		NetworkServer.SetClientReady(NetworkServer.localConnection);
		NetworkServer.ReplacePlayerForConnection(NetworkServer.localConnection, debugPlayer, true);
	}

	public override void OnServerAddPlayer(NetworkConnection conn)
	{
		//base.OnServerAddPlayer(conn);
	}
}
