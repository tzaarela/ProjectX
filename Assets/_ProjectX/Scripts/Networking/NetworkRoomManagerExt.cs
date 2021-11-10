using Mirror;
using Managers;
using UnityEngine;
using Player;

namespace Networking
{
	public class NetworkRoomManagerExt : NetworkRoomManager
	{
		private static bool gameHasStarted;

		public override void OnServerChangeScene(string newSceneName)
		{
			if (gameHasStarted)
				return;
			
			if (newSceneName != GameplayScene)
				return;
			
			print("RoomSlotsCount at GameStartUp: " + roomSlots.Count);
			
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
		
		public void ReturnToMainMenu()
		{
			if (NetworkServer.active && NetworkClient.isConnected)
			{
				print("StopHost");
				StopHost();
			}
			else
			{
				print("StopClient");
				StopClient();
			}
		}
		
		[Server]
		public void ReloadGameScene()
		{
			ServerChangeScene(RoomScene);
		}

		[Client]
		public void EndGame()
		{
			if (NetworkServer.active && NetworkClient.isConnected)
			{
				print("StopHost");
				// print("RoomSlotsCount at EndGame: " + roomSlots.Count);
				gameHasStarted = false;
				StopHost();
			}
			else
			{
				print("StopClient");
				StopClient();
			}
		}

		public override GameObject OnRoomServerCreateGamePlayer(NetworkConnection conn, GameObject roomPlayer)
		{
			Transform startPos = GetStartPosition();
			GameObject gamePlayer = startPos != null
				? Instantiate(playerPrefab, startPos.position, startPos.rotation)
				: Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

			return gamePlayer;
		}
		
		public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer, GameObject gamePlayer)
		{
			NetworkRoomPlayerExt networkedRoomPlayer = roomPlayer.GetComponent<NetworkRoomPlayerExt>();
			PlayerController playerController =  gamePlayer.GetComponent<PlayerController>();
			playerController.playerName = networkedRoomPlayer.playerName;
			playerController.playerMaterialIndex = networkedRoomPlayer.playerMaterialIndex;

			return true;
		}
	}
}