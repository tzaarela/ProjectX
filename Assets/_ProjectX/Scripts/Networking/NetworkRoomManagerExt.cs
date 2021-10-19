using Mirror;
using Managers;

namespace Networking
{
	public class NetworkRoomManagerExt : NetworkRoomManager
	{
		private const string GameScene = "Assets/_ProjectX/Scenes/Game.unity";

		//Before server loads Game-scene
		public override void ServerChangeScene(string newSceneName)
		{
			base.ServerChangeScene(newSceneName);
			
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
	}
}