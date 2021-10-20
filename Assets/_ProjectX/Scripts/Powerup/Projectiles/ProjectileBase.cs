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
		protected FMODUnity.StudioEventEmitter emitter;
		
		[SerializeField] protected float damage = 1f;
		[SerializeField] protected float shootingStrength = 100f;
		[SerializeField] protected float aliveTime = 2f;
		protected int spawnedByNetId;

		protected Rigidbody rb;

		protected Vector3 direction;
		
		private void Awake()
		{
			emitter = GetComponent<FMODUnity.StudioEventEmitter>();
			rb = GetComponent<Rigidbody>();
		}

		protected virtual void OnEnable() { }

		protected virtual void OnDisable()
		{
			rb.velocity = Vector3.zero;
			transform.eulerAngles = Vector3.zero;
			StopCoroutine(CoDestroyAfterTime());
		}

		[Server]
		public virtual void SetupProjectile(Vector3 dir, int netID)
		{
			direction = dir;
			spawnedByNetId = netID;
			
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