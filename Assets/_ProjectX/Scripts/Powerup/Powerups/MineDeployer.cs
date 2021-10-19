using Data.Enums;
using Managers;
using PowerUp.Projectiles;
using UnityEngine;

namespace Powerup.Powerups
{
	public class MineDeployer : PowerupBase
	{
		private void OnEnable()
		{
			ammo = 30;
		}

		protected override void Execute(int netID)
		{
			foreach (Transform hardpoint in hardpoints)
			{
				Vector3 direction = hardpoint.forward;
				
				Bullet bullet = ServiceLocator.ObjectPools.SpawnFromPool(ObjectPoolType.Bullet).GetComponent<Bullet>();
				bullet.transform.position = hardpoint.position + direction * 0.5f;
				bullet.SetupProjectile(direction, netID);
			}
		}
	}
}