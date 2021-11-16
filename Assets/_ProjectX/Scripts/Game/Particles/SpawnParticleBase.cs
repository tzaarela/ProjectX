using System.Collections;
using Data.Enums;
using Data.Interfaces;
using Managers;
using Mirror;
using UnityEngine;

namespace Game.Particles
{
	public class SpawnParticleBase : NetworkBehaviour, ISpawnedByID
	{
		protected int spawnedByNetId;
		protected ObjectPoolType currentPoolType;
		[SerializeField] protected float aliveTime;
		[SerializeField] protected ParticleSystem[] particlePrefabs;
		
		protected virtual void Awake()
		{
			if(particlePrefabs.Length <= 0)
				return;
			
			foreach (var particle in particlePrefabs)
			{
				float tempTime = particle.main.duration + particle.main.startLifetime.constantMax;

				if (aliveTime < tempTime)
					aliveTime = tempTime;
			}
		}
		
		protected virtual void OnEnable()
		{
			if (!GetComponentInChildren<ParticleSystem>())
			{
				foreach (var particle in particlePrefabs)
				{
					Instantiate(particle, transform);
				}
			}
			
			if(isServer)
				StartCoroutine(CoDestroyAfterTime());
		}
		
		public void SetSpawnedBy(int netID)
		{
			spawnedByNetId = netID;
		}

		public int GetSpawnedBy()
		{
			return spawnedByNetId;
		}
		
		private IEnumerator CoDestroyAfterTime()
		{
			yield return new WaitForSeconds(aliveTime);
			transform.position = Vector3.zero;
			transform.rotation = Quaternion.identity;
			ServiceLocator.ObjectPools.ReturnToPool(currentPoolType, gameObject);
		}
	}
}