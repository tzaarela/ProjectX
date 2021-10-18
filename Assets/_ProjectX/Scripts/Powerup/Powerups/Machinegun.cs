using Data.Enums;
using Managers;
using PowerUp.Projectiles;
using UnityEngine;

namespace Powerup.Powerups
{
	public class Machinegun : PowerupBase
	{
		private void OnEnable()
		{
			ammo = 40;
		}

		protected override void Execute()
		{
			base.Execute();
			
			if (ammo > 0)
			{
				foreach (Transform hardpoint in hardpoints)
				{
					Vector3 direction = hardpoint.forward;

					Bullet bullet = ServiceLocator.ObjectPools.SpawnFromPool(ObjectPoolType.Bullet).GetComponent<Bullet>();
					bullet.transform.position = hardpoint.position + direction * 0.5f;
					bullet.transform.rotation = hardpoint.rotation;
					bullet.SetupProjectile(direction);

					ammo--;
				}
			}
		}
	}
}