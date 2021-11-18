using Mirror;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using Managers;

public class ResetZone : NetworkBehaviour
{
	[Server]
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			other.GetComponent<PowerupController>().Drop();
			other.GetComponent<PlayerController>().DropFlag();
			other.GetComponent<Health>().ResetCurrentHealth();

			var attachedRigidbody = other.attachedRigidbody;
			attachedRigidbody.velocity = Vector3.zero;
			attachedRigidbody.angularVelocity = Vector3.zero;
				
			var playerTransform = other.transform;

			ServiceLocator.RespawnManager.RespawnPlayer(other.transform);
		}
	}
}
