﻿using Mirror;
using UnityEngine;

namespace Managers
{
	public class DestructionManager : NetworkBehaviour
	{
		public static DestructionManager Instance;
		
		private void Awake()
		{
			if (Instance == null)
				Instance = this;
			else
				Destroy(gameObject);
		}
		
		[Server]
		public void DestructObject(GameObject destructionObject, Vector3 impulse)
		{
			RpcDestructObject(destructionObject, impulse);
		}

		[ClientRpc]
		private void RpcDestructObject(GameObject destructionObject, Vector3 impulse)
		{
			destructionObject.GetComponent<DestructionObject>().Destruct(impulse);
		}
	}
}