using System;
using Data.Enums;
using Managers;
using Mirror;
using PowerUp.Projectiles;
using UnityEngine;
using System.Collections;

namespace Powerup.Powerups
{
	public class RocketLauncher : PowerupBase
	{
		[SerializeField]
		[FMODUnity.EventRef]
		private string reloadSound;
		private FMOD.Studio.EventInstance reloadSoundInstance;

		private bool localUseLocked;
		
		protected override void Start()
		{
			base.Start();

			forwardSpawnOffset = 1f;
			fireCooldown = 2.2f;
		}

		private void Awake()
		{
			reloadSoundInstance = FMODUnity.RuntimeManager.CreateInstance(reloadSound);
			reloadSoundInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
		}

		private void OnEnable()
		{
			ammo = 5;
		}

		public override void LocalUse()
		{
			base.LocalUse();

			if (localUseLocked)
				return;

			localUseLocked = true;
			
			StartCoroutine(CoCountReloadTime());
		}

		[Server]
		protected override void Execute(int netID)
		{
			base.Execute(netID);
			
			if (ammo > 0)
			{
				foreach (Transform hardpoint in hardpoints)
				{
					Vector3 direction = hardpoint.forward;
					
					Rocket rocket = ServiceLocator.ObjectPools.SpawnFromPoolWithNetId(ObjectPoolType.Rocket, hardpoint.position + direction, hardpoint.rotation, netID).GetComponent<Rocket>();
					
					if(rocket == null)
						continue;
					
					rocket.transform.position = hardpoint.position + direction * forwardSpawnOffset;
					rocket.transform.rotation = hardpoint.rotation;
					rocket.SetupProjectile(direction, netID);

					ammo--;

					ServiceLocator.HudManager.TargetUpdateAmmoUi(playerController.connectionToClient, ammo);
				}
			}
		}
		
		private IEnumerator CoCountReloadTime()
		{
			yield return new WaitForSeconds(fireCooldown);
			localUseLocked = false;
			reloadSoundInstance.start();
			FMODUnity.RuntimeManager.AttachInstanceToGameObject(reloadSoundInstance, transform);
		}
	}
}