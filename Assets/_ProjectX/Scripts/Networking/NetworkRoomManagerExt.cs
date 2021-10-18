using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Networking
{
	public class NetworkRoomManagerExt : NetworkRoomManager
	{
		public override void OnServerSceneChanged(string sceneName)
		{
			base.OnServerSceneChanged(sceneName);
		}

		//Game Starts!
		public override void ServerChangeScene(string newSceneName)
		{
			base.ServerChangeScene(newSceneName);

			foreach (NetworkRoomPlayer player in roomSlots)
			{
				if (player.readyToBegin)
					print("name: " + player.name + "Id: " + player.netId);
			}
		}
	}
}