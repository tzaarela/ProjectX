using Audio;
using Mirror;
using Managers;
using UnityEngine;
using Player;

namespace Networking
{
	public class NetworkRoomManagerExt : NetworkRoomManager
	{
		public static string serverAdress = "localhost";

		[Server]
		public override void OnRoomServerPlayersReady()
		{
			if (roomSlots.Count == 0)
				return;

			int connectedClients = 0;
			foreach (NetworkRoomPlayer player in roomSlots)
			{
				if (player.readyToBegin)
				{
					// print("name: " + player.name + "Id: " + player.netId);
					connectedClients++;
				}
			}
			print("Connected clients when starting game: " + connectedClients);
			ServiceLocator.RoundManager.NumberOfConnectedClients = connectedClients;
			
			ServiceLocator.LobbyManager.StartGameButton.SetActive(true);
		}
		
		[Server]
		public override void OnRoomServerPlayersNotReady()
		{
			if (!IsSceneActive(RoomScene))
				return;

			ServiceLocator.LobbyManager.StartGameButton.SetActive(false);
		}

		[Server]
		public void StartGame()
		{
			ServerChangeScene(GameplayScene);
		}

		[Client]
		public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
		{
			if (newSceneName != GameplayScene)
				return;

			FMODUnity.RuntimeManager.PlayOneShot("event:/UI/StartGame", Camera.main.transform.position);
			
			GameObject musicPlayer = GameObject.Find("MenuMusicPlayer");

			if (musicPlayer == null)
				return;

			if (musicPlayer.TryGetComponent(out MenuMusicController menuMusicController))
			{
				menuMusicController.Destroy();
			}
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
				// gameHasStarted = false;
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

			//Turn off network transform so we dont sync this when in match, reduces bandwith load.
			networkedRoomPlayer.GetComponent<NetworkTransform>().enabled = false;

			return true;
		}
	}
}