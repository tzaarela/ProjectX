using Data.Enums;
using Data.Interfaces;
using Managers;
using Mirror;
using PowerUp.Projectiles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Containers;
using Data.Containers.GlobalSignal;
using FMOD.Studio;
using UnityEngine;
using Player;

namespace Powerup.Projectiles
{
	public class Mine : NetworkBehaviour, ISpawnedByID, ISendGlobalSignal
	{
		public float timeUntilArmed = 1.5f;
		public float triggerDelay = 0.5f;
		public bool shouldTakeSelfDamage = true;

		[SerializeField]
		[FMODUnity.EventRef]
		private string deploySound;
    
		private FMOD.Studio.EventInstance deploySoundInstance;
		
		[SerializeField] ParticleSystem lightParticle;

		[SyncVar(hook = nameof(ToggleLight))] private bool isArmed;

		private int spawnedByNetId;
		private Rigidbody rb;

		private void Awake()
		{
			deploySoundInstance = FMODUnity.RuntimeManager.CreateInstance(deploySound);
		}

		private void OnEnable()
		{
			if (isServer)
			{
				Invoke(nameof(Arm), timeUntilArmed);
			}

			deploySoundInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform, rb));
			deploySoundInstance.start();
		}

		private void OnDisable()
		{
			if (isServer)
				isArmed = false;
			
			deploySoundInstance.stop(STOP_MODE.ALLOWFADEOUT);

		}

		[Server]
		private void Arm()
		{
			isArmed = true;
		}

		//Hook
		[Client]
		private void ToggleLight(bool oldValue, bool turnOn)
		{
			var lightParticleMain = lightParticle.main;
			if (turnOn)
				lightParticleMain.startColor = Color.red;
			else
				lightParticleMain.startColor = Color.green;

		}
		
		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Player") && isArmed && isClient)
			{
				GameObject player = NetworkClient.connection.identity.gameObject;
				float distance = Vector3.Distance(transform.position, player.transform.position);
				
				SendGlobal(GlobalEvent.CAMERA_SHAKE, new CameraShakeData(2f, 0.3f, distance));
			}

			if (!isServer)
				return;

			if (other.CompareTag("Player") && isArmed)
			{
				if (spawnedByNetId == other.gameObject.GetComponent<PlayerController>().PlayerId && !shouldTakeSelfDamage)
					return;

				StartCoroutine(CoWaitForExplosionTrigger());
			}
		}

		private IEnumerator CoWaitForExplosionTrigger()
		{
			yield return new WaitForSeconds(triggerDelay);
			ServiceLocator.ObjectPools.SpawnFromPoolWithNetId(ObjectPoolType.MineExplosion, transform.position, Quaternion.identity, spawnedByNetId);
			ServiceLocator.ObjectPools.ReturnToPool(ObjectPoolType.Mine, gameObject);
		}

		public void SetSpawnedBy(int netID)
		{
			spawnedByNetId = netID;
		}

		public int GetSpawnedBy()
		{
			return spawnedByNetId;
		}

		public void SendGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			GlobalMediator.Instance.ReceiveGlobal(eventState, globalSignalData);
		}
	}
}
