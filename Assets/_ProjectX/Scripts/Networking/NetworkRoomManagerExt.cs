using Mirror;
using Managers;
using UnityEngine.SceneManagement;

namespace Networking
{
	public class NetworkRoomManagerExt : NetworkRoomManager
	{
		private const string GameScene = "Assets/_ProjectX/Scenes/Game.unity";

		//Called just before Server loads new scene
		public override void OnServerChangeScene(string newSceneName)
		{
			if (newSceneName != GameScene)
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
		}

		[Server]
		public void ReloadGameScene()
		{
			ServerChangeScene(GameScene);
		}

		[Client]
		public void LoadMainMenuScene()
		{
			//Let Server/Host know that client leaves Game/Connection..?
			SceneManager.LoadScene("MainMenu");
		}
		
	}
}