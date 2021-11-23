using System;
using Mirror;
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
		
		[ServerCallback]
		public void DestructObject(GameObject destructionObject, Vector3 impulse)
		{
			RpcDestructObject(destructionObject, impulse);
		}
		
		[ServerCallback]
		public void PlayerDestructObject(GameObject destructionObject)
		{
			RpcPlayerDestructObject(destructionObject);
		}

		[ClientRpc]
		private void RpcDestructObject(GameObject destructionObject, Vector3 impulse)
		{
			destructionObject.GetComponent<DestructionObject>().Destruct(impulse);
		}
		
		[ClientRpc]
		private void RpcPlayerDestructObject(GameObject destructionObject)
		{
			destructionObject.GetComponent<DestructionObject>().Explode();
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!isServer)
				return;
			
			if (other.CompareTag("DestructionObject"))
			{
				NetworkServer.Destroy(other.gameObject);
				Destroy(other.gameObject);
			}
		}
	}
}