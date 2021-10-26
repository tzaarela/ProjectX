using Mirror;
using Managers;
using UnityEngine;
using Player;
using Data.Interfaces;
using Data.Enums;
using Data.Containers.GlobalSignal;

namespace Networking
{
	public class NetworkRoomManagerExt : NetworkRoomManager, IReceiveGlobalSignal
	{
		private static bool gameHasStarted;

		public override void OnServerSceneChanged(string sceneName)
		{
			base.OnServerSceneChanged(sceneName);

			if (sceneName == GameplayScene)
				GlobalMediator.Instance.Subscribe(this);
		}

		public override void OnRoomServerSceneChanged(string sceneName)
		{
			base.OnRoomServerSceneChanged(sceneName);

			if (sceneName == GameplayScene)
				GlobalMediator.Instance.Subscribe(this);
		}

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
			playerController.playerColor = networkedRoomPlayer.playerColor;
			
			return true;
		}

		public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			if (eventState == GlobalEvent.ALL_PLAYERS_CONNECTED_TO_GAME)
			{
				//for (int i = roomSlots.Count - 1; i >= 0; i--)
				//{
				//	NetworkServer.Destroy(roomSlots[i].gameObject);
				//}
			}
		}
	}
}