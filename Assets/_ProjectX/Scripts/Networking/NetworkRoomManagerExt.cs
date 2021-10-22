using Mirror;
using Managers;
using UnityEngine;

namespace Networking
{
	public class NetworkRoomManagerExt : NetworkRoomManager
	{
		private static bool gameHasStarted;
		
		//Called just before Server loads new scene
		public override void OnServerChangeScene(string newSceneName)
		{
			if (gameHasStarted)
				return;
			
			if (newSceneName != GameplayScene)
				return;
			
			int connectedClients = 0;
			foreach (NetworkRoomPlayer player in roomSlots)
			{
				if (player.readyToBegin)
				{
					print("name: " + player.name + "Id: " + player.netId);
					connectedClients++;
				}
			}
			print("Connected clients when starting game: " + connectedClients);
			ServiceLocator.RoundManager.NumberOfConnectedClients = connectedClients;
			gameHasStarted = true;
		}

		[Server]
		public void ReloadGameScene()
		{
			ServerChangeScene(GameplayScene);
		}

		[Server]
		public override void OnServerDisconnect(NetworkConnection conn)
		{
			base.OnServerDisconnect(conn);

			if (!IsSceneActive(GameplayScene))
				return;
			
			StopHost();
		}
		
		public void EndGame()
		{
			gameHasStarted = false;
			
			StopClient();
		}

		public override GameObject OnRoomServerCreateGamePlayer(NetworkConnection conn, GameObject roomPlayer)
		{
			NetworkServer.Destroy(roomPlayer);
			return base.OnRoomServerCreateGamePlayer(conn, roomPlayer);
		}

		public override void OnServerAddPlayer(NetworkConnection conn)
		{
			base.OnServerAddPlayer(conn);

			ServiceLocator.LobbyManager.AddRoomPlayer(conn.identity.gameObject.GetComponent<NetworkRoomPlayerExt>());
		}
	}
}