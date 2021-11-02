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
			foreach (Transform hardpoint in hardpoints)
			{
				Mine mine = ServiceLocator.ObjectPools.SpawnFromPool(ObjectPoolType.Mine).GetComponent<Mine>();
				mine.transform.position = hardpoint.position;
			}
		}
	}
}