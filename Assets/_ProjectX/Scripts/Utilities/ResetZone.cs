using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetZone : NetworkBehaviour
{
	[SerializeField] private Transform resetPoint;

	[Server]
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			other.attachedRigidbody.velocity = Vector3.zero;
			other.transform.rotation = Quaternion.identity;
			other.transform.position = resetPoint.position;
		}
	}
}
