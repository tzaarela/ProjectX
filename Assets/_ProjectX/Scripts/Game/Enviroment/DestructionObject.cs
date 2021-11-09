using System;
using System.Collections.Generic;
using Data.Interfaces;
using Managers;
using Mirror;
using PowerUp.Projectiles;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(NetworkIdentity))]
public class DestructionObject : NetworkBehaviour, IReceiveDamageAOE
{
	private Rigidbody rb;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void Start()
	{
		rb.isKinematic = true;
		
		DeactivateColliders();
	}

	private void DeactivateColliders()
	{
		foreach (Collider coll in GetComponentsInChildren<Collider>())
		{
			if(!coll.isTrigger)
				coll.enabled = false;
		}
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

		if (!isServer)
			return;
		
		DestructionManager.Instance.DestructObject(gameObject, other.relativeVelocity);
	}

	private void OnTriggerEnter(Collider other)
	{
		if(!rb.isKinematic)
			return;
		
		if (!isServer)
			return;
		
		// if (other.CompareTag("Player"))
		// {
		// 	rb.isKinematic = false;
		// 	
		// 	DestructionManager.Instance.PlayerDestructObject(gameObject);
		// }
		
		if (other.CompareTag("Player"))
		{
			rb.isKinematic = false;
			
			DestructionManager.Instance.DestructObject(gameObject, other.attachedRigidbody.velocity);
			
			foreach (Collider coll in GetComponentsInChildren<Collider>())
			{
				coll.enabled = true;
			}
		}
		else if (other.CompareTag("Projectile"))
		{
			rb.isKinematic = false;
			
			foreach (Collider coll in GetComponentsInChildren<Collider>())
			{
				coll.enabled = true;
			}
			
			DestructionManager.Instance.DestructObject(gameObject, other.attachedRigidbody.velocity);

			other.GetComponent<ProjectileBase>().OverrideCollision();
		}
	}

	public void ReceiveDamageAOE(Vector3 direction, float distance, int damage, int spawnedById)
	{
		DestructionManager.Instance.DestructObject(gameObject, direction * (distance * 10));
	}
}
