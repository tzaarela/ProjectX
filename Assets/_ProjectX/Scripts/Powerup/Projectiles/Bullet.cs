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

		private Vector3 startPosition;
		[SerializeField] private LayerMask raycastLayerMask;
		
		[SerializeField]
		[FMODUnity.EventRef]
		private string shootSound;

		private FMOD.Studio.EventInstance shootSoundInstance;
		
		protected override void Start()
		{
			base.Start();

			currentPoolType = ObjectPoolType.Bullet;
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			startPosition = transform.position;
			
			trailRenderer.Clear();
			direction = transform.forward;
			rb.AddForce(direction * shootingStrength, ForceMode.Impulse);
			
			if (!shootSoundInstance.hasHandle())
				shootSoundInstance = FMODUnity.RuntimeManager.CreateInstance(shootSound);
			
			PlayShootSound();
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
					//Debug.Log("BULLET COLLIDED WITH: " + other.gameObject.name, other.gameObject);
					//Instantiate(debugSphere, transform.position, quaternion.identity);
					FMODUnity.RuntimeManager.PlayOneShot("event:/Weapons/HitReg", Camera.main.transform.position);
				}
			}
			// else
			// {
			// 	foreach (var contact in other.contacts)
			// 	{
			// 		Instantiate(debugSphere, other.GetContact(0).point - transform.forward * 0.8f, Quaternion.identity);
			// 	}
			// }
			
			if(!isServer)
				return;

			if (other.gameObject.CompareTag("Player"))
			{
				PlayerController playerController = other.gameObject.GetComponent<PlayerController>();

				if (spawnedByNetId == (int)playerController.netId)
					return;
				
				other.gameObject.GetComponent<Health>().ReceiveDamage(10, spawnedByNetId);
			}
			
			//TODO Find better way to get point of impact
			Vector3 contactOffset = other.GetContact(0).point - transform.forward * 1.1f;
			ServiceLocator.ObjectPools.SpawnFromPoolWithNetId(ObjectPoolType.BulletHitEffect, contactOffset, Quaternion.identity, spawnedByNetId);

			allowCollision = false;
			ServiceLocator.ObjectPools.ReturnToPool(ObjectPoolType.Bullet, gameObject);
		}

		public override void OverrideCollision()
		{
			base.OverrideCollision();
			
			allowCollision = false;
			ServiceLocator.ObjectPools.ReturnToPool(ObjectPoolType.Bullet, gameObject);
		}
		
		private void PlayShootSound()
		{
			shootSoundInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
			shootSoundInstance.start();
		}
	}
}