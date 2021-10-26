using Data.Enums;
using Managers;
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

		protected override void Execute(int netID)
		{
			base.Execute(netID);
			
			if (ammo > 0)
			{
				foreach (Transform hardpoint in hardpoints)
				{
					Vector3 direction = hardpoint.forward;

					Bullet bullet = ServiceLocator.ObjectPools.SpawnFromPool(ObjectPoolType.Bullet, hardpoint.position + direction * forwardSpawnOffset, hardpoint.rotation).GetComponent<Bullet>();

					if(bullet == null)
						continue;
					
					bullet.transform.position = hardpoint.position + direction * forwardSpawnOffset;
					bullet.transform.rotation = hardpoint.rotation;
					bullet.SetupProjectile(direction, netID);

					ammo--;
				}
			}
		}
	}
}