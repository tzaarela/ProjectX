using Data.Enums;
using Managers;
using Powerup.Projectiles;
using PowerUp.Projectiles;
using UnityEngine;

namespace Powerup.Powerups
{
	public class MineDeployer : PowerupBase
	{
		private void OnEnable()
		{
			ammo = 2;
		}

		protected override void Execute(int netID)
		{
			if (ammo <= 0)
				return;

			foreach (Transform hardpoint in hardpoints)
			{
				Mine mine = ServiceLocator.ObjectPools.
					SpawnFromPoolWithNetId(ObjectPoolType.Mine, hardpoint.position, Quaternion.identity, netID).GetComponent<Mine>();
				
				mine.transform.position = hardpoint.position;
				ammo--;

				ServiceLocator.HudManager.TargetUpdateAmmoUi(playerController.connectionToClient, ammo);
			}
		}
	}
}