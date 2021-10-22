using System;
using Data.Enums;
using Managers;
using Player;
using UnityEngine;

namespace PowerUp.Projectiles
{
	public class Rocket : ProjectileBase
	{
		public TrailRenderer trailerRenderer;

		private void Start()
		{
			//trailerRenderer.enabled = false;
		}

		public override void SetupProjectile(Vector3 dir, int netID)
		{
			base.SetupProjectile(dir, netID);

			//trailerRenderer.enabled = true;
			
			rb.AddForce(direction * shootingStrength, ForceMode.Impulse);
		}

		protected override void OnDisable()
		{
			base.OnDisable();


			//trailerRenderer.Clear();
			//trailerRenderer.enabled = false;
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
				
				other.gameObject.GetComponent<Health>().ReceiveDamage(50, spawnedByNetId);
			}
			
			ServiceLocator.ObjectPools.ReturnToPool(ObjectPoolType.Bullet, gameObject);
		}
	}
}