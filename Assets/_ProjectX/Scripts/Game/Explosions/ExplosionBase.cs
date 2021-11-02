using System.Collections;
using Data.Enums;
using Data.Interfaces;
using Managers;
using Mirror;
using UnityEngine;

namespace Game.Explosions
{
	[RequireComponent(typeof(FMODUnity.StudioEventEmitter))]
	public class ExplosionBase : NetworkBehaviour, ISpawnedByID
	{
		protected int spawnedByNetId;

		[SerializeField] protected float aoeRadius;
		[SerializeField] protected LayerMask aoeLayerMask;
		[SerializeField] protected ParticleSystem[] aoeParticlePrefabs;
		
		protected ObjectPoolType currentPoolType;
		[SerializeField] protected int damage;
		[SerializeField] protected float aoeAliveTime;
		
		protected virtual void Awake()
		{
			if(aoeParticlePrefabs.Length <= 0)
				return;
			
			foreach (var particle in aoeParticlePrefabs)
			{
				float tempTime = particle.main.duration + particle.main.startLifetime.constantMax;

				if (aoeAliveTime < tempTime)
					aoeAliveTime = tempTime;
			}
		}

		protected virtual void OnEnable()
		{
			if (!GetComponentInChildren<ParticleSystem>())
			{
				foreach (var particle in aoeParticlePrefabs)
				{
					Instantiate(particle, transform);
				}
			}
			
			StartCoroutine(CoDestroyAfterTime());
		}
		
		protected virtual void OnDisable() { }
		
		public void SetSpawnedBy(int netID)
		{
			spawnedByNetId = netID;
		}

		public int GetSpawnedBy()
		{
			return spawnedByNetId;
		}
		
		protected virtual void ProcessAOE()
		{
			Collider[] hitColliders = Physics.OverlapSphere(transform.position, aoeRadius, aoeLayerMask);
			
			foreach (Collider coll in hitColliders)
			{
				if (coll.TryGetComponent(out IReceiveDamageAOE receiver))
				{
					Vector3 position = transform.position;
					Vector3 collPosition = coll.transform.position;
					Vector3 dir = (collPosition - position).normalized;
					
					receiver.ReceiveDamageAOE(dir, Vector3.Distance(position, collPosition), damage);

					//coll.GetComponent<DestructionObject>().ReceiveDamageAOE(dir, Vector3.Distance(position, collPosition), damage);
				}
			}
		}
		
		private IEnumerator CoDestroyAfterTime()
		{
			yield return new WaitForSeconds(aoeAliveTime);
			transform.position = Vector3.zero;
			transform.rotation = Quaternion.identity;
			ServiceLocator.ObjectPools.ReturnToPool(currentPoolType, gameObject);
		}
		
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(transform.position, aoeRadius);
		}
	}
}