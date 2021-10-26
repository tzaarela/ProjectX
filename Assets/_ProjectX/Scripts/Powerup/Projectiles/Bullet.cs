using System;
using Data.Enums;
using Managers;
using Mirror;
using Player;
using Unity.Mathematics;
using UnityEngine;

namespace PowerUp.Projectiles
{
	public class Bullet : ProjectileBase
	{
		public GameObject debugSphere;
		public TrailRenderer trailRenderer;

		protected override void Start()
		{
			base.Start();

			currentPoolType = ObjectPoolType.Bullet;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			
			trailRenderer.Clear();
			direction = transform.forward;
			rb.AddForce(direction * shootingStrength, ForceMode.Impulse);
		}

		public override void SetupProjectile(Vector3 dir, int netID)
		{
			base.SetupProjectile(dir, netID);
		}
		
		private void OnCollisionEnter(Collision other)
		{
			if(!allowCollision)
				return;
			
			if (other.gameObject.CompareTag("Player"))
			{
				if (other.gameObject.GetComponent<PlayerController>().PlayerId != spawnedByNetId)
				{
					Debug.Log("BULLET COLLIDED WITH: " + other.gameObject.name, other.gameObject);
					//Instantiate(debugSphere, transform.position, quaternion.identity);
					FMODUnity.RuntimeManager.PlayOneShot("event:/Weapons/HitReg", Camera.main.transform.position);
				}
			}
			
			if(!isServer)
				return;

			if (other.gameObject.CompareTag("Player"))
			{
				PlayerController playerController = other.gameObject.GetComponent<PlayerController>();

				if (spawnedByNetId == (int)playerController.netId)
					return;
				
				other.gameObject.GetComponent<Health>().ReceiveDamage(10, spawnedByNetId);
			}

			allowCollision = false;
			ServiceLocator.ObjectPools.ReturnToPool(ObjectPoolType.Bullet, gameObject);
		}
	}
}