using Data.Enums;
using Managers;
using Player;
using UnityEngine;

namespace PowerUp.Projectiles
{
	public class Rocket : ProjectileBase
	{
		public TrailRenderer trailerRenderer;

		protected override void Start()
		{
			base.Start();

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

				if(spawnedByNetId == (int)playerController.netId)
					return;
				
				other.gameObject.GetComponent<Health>().ReceiveDamage(50, spawnedByNetId);
				
				allowCollision = false;
				ServiceLocator.ObjectPools.ReturnToPool(ObjectPoolType.Rocket, gameObject);

				return;
			}

			FMODUnity.RuntimeManager.PlayOneShot("event:/Weapons/Explosion", Camera.main.transform.position);
			
			allowCollision = false;
			ServiceLocator.ObjectPools.ReturnToPool(ObjectPoolType.Rocket, gameObject);
		}
	}
}