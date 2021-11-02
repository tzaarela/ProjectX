using Data.Enums;
using Data.Interfaces;
using Managers;
using Mirror;
using PowerUp.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Powerup.Projectiles
{
	public class Mine : NetworkBehaviour, ISpawnedByID
	{
		public float timeUntilActivaded = 1.5f;
		public bool isArmed;

		[SerializeField] ParticleSystem lightParticle;

		[SyncVar(hook = nameof(Arm))] private bool isActivated;

		private int spawnedByNetId;

		private void Start()
		{
			if(isServer)
				Invoke(nameof(Activate), timeUntilActivaded);
		}

		[Server]
		private void Arm(bool oldValue, bool newValue)
		{
			var lightParticleMain = lightParticle.main;
			lightParticleMain.startColor = Color.red;
			isArmed = true;
		}

		[Server]
		private void Activate()
		{
			isActivated = true;
		}

		
		private void OnTriggerEnter(Collider other)
		{
			if (!isServer)
				return;

			if (other.CompareTag("Player") && isArmed)
			{
				ServiceLocator.ObjectPools.SpawnFromPoolWithNetId(ObjectPoolType.MineExplosion, transform.position, Quaternion.identity, spawnedByNetId);
				NetworkServer.Destroy(gameObject);
			}
		}

		public void SetSpawnedBy(int netID)
		{
			spawnedByNetId = netID;
		}

		public int GetSpawnedBy()
		{
			return spawnedByNetId;
		}
	}
}
