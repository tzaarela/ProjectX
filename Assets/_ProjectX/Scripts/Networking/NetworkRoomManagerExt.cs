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
	}
}