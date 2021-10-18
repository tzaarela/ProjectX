using System;
using System.Collections;
using Data.Enums;
using UnityEngine;
using Managers;
using Mirror;

namespace PowerUp.Projectiles
{
	public abstract class ProjectileBase : NetworkBehaviour
	{
		[SerializeField] protected float damage = 1f;
		[SerializeField] protected float shootingStrength = 100f;
		[SerializeField] protected float aliveTime = 2f;

		protected Rigidbody rb;

		protected Vector3 direction;

		public override void OnStartServer()
		{
			base.OnStartServer();
			rb = GetComponent<Rigidbody>();
		}
		
		private void OnDisable()
		{
			// TODO RESET VELOCITY AND STUFF
		}

		[Server]
		public virtual void SetupProjectile(Vector3 dir)
		{
			direction = dir;
			
			StartCoroutine(CoDestroyAfterTime());
		}

		private IEnumerator CoDestroyAfterTime()
		{
			yield return new WaitForSeconds(aliveTime);
			rb.velocity = Vector3.zero;
			transform.position = Vector3.zero;
			transform.rotation = Quaternion.identity;
			ServiceLocator.ObjectPools.ReturnToPool(ObjectPoolType.Bullet, gameObject);
		}
	}
}