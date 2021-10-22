using System;
using Managers;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(NetworkIdentity))]
public class DestructionObject : MonoBehaviour
{
	private Rigidbody rb;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void Start()
	{
		rb.isKinematic = true;
	}

	public void Destruct(Vector3 impulse)
	{
		if(rb == null)
			return;
		
		rb.isKinematic = false;
		rb.AddForce(impulse, ForceMode.Impulse);
	}
	
	public void Explode()
	{
		if(rb == null)
			return;
		
		rb.isKinematic = false;
		rb.AddExplosionForce(100, transform.position, 3, 3f); //AddForce(impulse, ForceMode.Impulse);
	}

	private void OnCollisionEnter(Collision other)
	{
		if(!rb.isKinematic)
			return;

		DestructionManager.Instance.DestructObject(gameObject, other.relativeVelocity);
	}

	private void OnTriggerEnter(Collider other)
	{
		if(rb.isKinematic)
			return;
		
		if (other.CompareTag("Player"))
		{
			rb.isKinematic = false;
			
			DestructionManager.Instance.PlayerDestructObject(gameObject);
		}
	}
}
