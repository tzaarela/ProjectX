using Mirror;
using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace Networking
{
	public class NetworkRoomManagerExt : NetworkRoomManager
	{
		private const string GameScene = "Assets/_ProjectX/Scenes/Game.unity";
		
		public override void OnServerSceneChanged(string sceneName)
		{
			base.OnServerSceneChanged(sceneName);
		}

		//Game Starts!
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