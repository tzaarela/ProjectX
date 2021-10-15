using System;
using System.Collections;
using Data.Enums;
using UnityEngine;
using Managers;

namespace PowerUp.Projectiles
{
	public abstract class ProjectileBase : MonoBehaviour
	{
		protected float damage;
		protected float shootingStrength;
		protected float aliveTime;

		protected Rigidbody rb;

		protected Vector3 direction;
		
		protected virtual void Start()
		{
			rb.GetComponent<Rigidbody>();
		}
		
		private void OnDisable()
		{
			// TODO RESET VELOCITY AND STUFF
		}

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