using System;
using System.Collections;
using Data.Enums;
using Data.Interfaces;
using UnityEngine;
using Managers;
using Mirror;

namespace PowerUp.Projectiles
{
	public abstract class ProjectileBase : NetworkBehaviour, ISpawnedByID
	{
		private FMODUnity.StudioEventEmitter emitter;
		
		[SerializeField] protected int directDamage = 1;
		[SerializeField] protected float shootingStrength = 100f;
		[SerializeField] protected float aliveTime = 2f;
		protected int spawnedByNetId;
		protected ObjectPoolType currentPoolType;
		protected bool allowCollision;
		
		protected Rigidbody rb;

		protected Vector3 direction;
		
		protected virtual void Awake()
		{
			emitter = GetComponent<FMODUnity.StudioEventEmitter>();
			rb = GetComponent<Rigidbody>();
		}

		protected virtual void Start() { }

		protected virtual void OnEnable()
		{
			allowCollision = true;
		}

		protected virtual void OnDisable()
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			transform.eulerAngles = Vector3.zero;
			allowCollision = false;
			SetSpawnedBy(-1);
			StopCoroutine(CoDestroyAfterTime());
		}

		public void SetSpawnedBy(int netID)
		{
			spawnedByNetId = netID;
		}

		public int GetSpawnedBy()
		{
			return spawnedByNetId;
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
			ServiceLocator.ObjectPools.ReturnToPool(currentPoolType, gameObject);
		}

		public virtual void OverrideCollision()
		{
			
		}
	}
}