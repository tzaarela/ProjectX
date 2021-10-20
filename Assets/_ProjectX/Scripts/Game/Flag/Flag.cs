using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Player;

namespace Game.Flag
{
	public class Flag : NetworkBehaviour
	{
		public Action onFlagPickedUp;

		[SerializeField] private BoxCollider pickupCollider;
		[SerializeField] private BoxCollider physicsCollider;
		
		private Rigidbody rb;

		private void Start()
		{
			rb = GetComponent<Rigidbody>();
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!isServer)
			{
				return;
			}

			if (other.gameObject.CompareTag("Player"))
			{
				PlayerController player = other.gameObject.GetComponent<PlayerController>();
				PickUp(player);
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			//if (!isServer)
			//{
			//	return;
			//}

			//if (collision.gameObject.CompareTag("Player"))
			//{
			//	PlayerController player = collision.gameObject.GetComponent<PlayerController>();
			//	PickUp(player);
			//}
		}

		public void TogglePhysics(bool turnOn) 
		{
			physicsCollider.enabled = turnOn;
		}

		[Server]
		public void PickUp(PlayerController playerPickingUp)
		{
			playerPickingUp.GiveFlag(this);
			RpcDeactivateFlag();
			onFlagPickedUp();
		}
		
		[Server]
		public void Drop()
		{
			physicsCollider.enabled = true;
			rb.isKinematic = false;
			RpcActivateFlag();
		}

		[ClientRpc]
		public void RpcDeactivateFlag() 
		{ 
			gameObject.SetActive(false);
		}

		[ClientRpc]
		public void RpcActivateFlag()
		{
			gameObject.SetActive(true);
		}
	}
}
