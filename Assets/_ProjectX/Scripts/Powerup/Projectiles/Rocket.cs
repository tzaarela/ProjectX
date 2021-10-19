using System;
using Data.Enums;
using Managers;
using Player;
using UnityEngine;

namespace PowerUp.Projectiles
{
	public class Rocket : ProjectileBase
	{
		public override void SetupProjectile(Vector3 dir, int netID)
		{
			base.SetupProjectile(dir, netID);
			
			rb.AddForce(direction * shootingStrength, ForceMode.Impulse);
		}
		
		private void OnCollisionEnter(Collision other)
		{
			if(!isServer)
				return;

			if (other.gameObject.CompareTag("Player"))
			{
				PlayerController playerController = other.gameObject.GetComponent<PlayerController>();

				if(spawnedByNetId == (int)playerController.netId)
					return;
				
				//TODO playerController.DealDamage(bulletOwner, damageType);
			}
			
			ServiceLocator.ObjectPools.ReturnToPool(ObjectPoolType.Bullet, gameObject);
		}
	}
}