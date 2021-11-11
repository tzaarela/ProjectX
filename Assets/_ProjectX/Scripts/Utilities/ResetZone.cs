using Mirror;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

public class ResetZone : NetworkBehaviour
{
	[SerializeField] private Transform resetPoint;

	[Server]
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			other.GetComponent<PowerupController>().Drop();
			other.GetComponent<PlayerController>().DropFlag();

			var attachedRigidbody = other.attachedRigidbody;
			attachedRigidbody.velocity = Vector3.zero;
			attachedRigidbody.angularVelocity = Vector3.zero;
			
			var otherTransform = other.transform;
			otherTransform.rotation = Quaternion.identity;
			otherTransform.position = resetPoint.position;
			
		}
	}
}
