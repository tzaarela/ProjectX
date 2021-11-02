using Data.Enums;
using Managers;
using Mirror;
using PowerUp.Projectiles;
using UnityEngine;

namespace Powerup.Powerups
{
	public class Machinegun : PowerupBase
	{
		protected override void Start()
		{
			base.Start();

			forwardSpawnOffset = 0.0f;
			heightSpawnOffset = -0.5f;
			fireCooldown = 0.2f;
		}

		private void OnEnable()
		{
			ammo = 40;
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

					Bullet bullet = ServiceLocator.ObjectPools.SpawnFromPoolWithNetId(ObjectPoolType.Bullet, hardpoint.position + direction, hardpoint.rotation, netID).GetComponent<Bullet>();

					if(bullet == null)
						continue;
					
					bullet.transform.position = hardpoint.position + direction * forwardSpawnOffset;
					bullet.transform.rotation = hardpoint.rotation;
					bullet.SetupProjectile(direction, netID);

					ammo--;
					
					ServiceLocator.HudManager.TargetUpdateAmmoUi(playerController.connectionToClient, ammo);
				}
			}
		}
	}
}