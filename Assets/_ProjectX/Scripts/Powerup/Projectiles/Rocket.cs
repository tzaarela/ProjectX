using System;
using System.Collections.Generic;
using Data.Enums;
using Data.Interfaces;
using Managers;
using Player;
using UnityEngine;
using Utilites;

namespace PowerUp.Projectiles
{
	public class Rocket : ProjectileBase
	{
		public TrailRenderer trailerRenderer;
		
		protected override void Start()
		{
			base.Start();

			directDamage = 50;

			currentPoolType = ObjectPoolType.Rocket;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			
			trailerRenderer.Clear();
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
					Debug.Log("ROCKET COLLIDED WITH: " + other.gameObject.name, other.gameObject);
					ServiceLocator.ObjectPools.SpawnFromPoolWithNetId(ObjectPoolType.RocketExplosion, transform.position, Quaternion.identity, spawnedByNetId);
				}
			}
			
			if(!isServer)
				return;

			if (other.gameObject.CompareTag("Player"))
			{
				PlayerController playerController = other.gameObject.GetComponent<PlayerController>();

				if(spawnedByNetId == (int)playerController.netId)
					return;
				
				other.gameObject.GetComponent<Health>().ReceiveDamage(directDamage, spawnedByNetId);
				
				allowCollision = false;
				ServiceLocator.ObjectPools.ReturnToPool(ObjectPoolType.Rocket, gameObject);

				return;
			}
			
			ServiceLocator.ObjectPools.SpawnFromPoolWithNetId(ObjectPoolType.RocketExplosion, transform.position, Quaternion.identity, spawnedByNetId);

			allowCollision = false;
			ServiceLocator.ObjectPools.ReturnToPool(ObjectPoolType.Rocket, gameObject);
		}
	}
}