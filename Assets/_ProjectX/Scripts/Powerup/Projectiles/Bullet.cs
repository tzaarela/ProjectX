using System;
using Data.Enums;
using Managers;
using Mirror;
using Player;
using UnityEngine;

namespace PowerUp.Projectiles
{
	public class Bullet : ProjectileBase
	{
		private TrailRenderer trailRenderer;

		protected override void Awake()
		{
			base.Awake();
			
			trailRenderer = GetComponent<TrailRenderer>();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			//trailRenderer.Clear();
		}

		public override void SetupProjectile(Vector3 dir, int netID)
		{
			base.SetupProjectile(dir, netID);
		}

		private void OnEnable()
		{
			direction = transform.forward;
			rb.AddForce(direction * shootingStrength, ForceMode.Impulse);
		}

		private void OnCollisionEnter(Collision other)
		{
			if (other.gameObject.CompareTag("Player"))
			{
				Debug.Log("BULLET COLLIDED WITH: " + other.gameObject.name, other.gameObject);
				FMODUnity.RuntimeManager.PlayOneShot("event:/Weapons/HitReg");
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
			
			ServiceLocator.ObjectPools.ReturnToPool(ObjectPoolType.Bullet, gameObject);
		}
	}
}