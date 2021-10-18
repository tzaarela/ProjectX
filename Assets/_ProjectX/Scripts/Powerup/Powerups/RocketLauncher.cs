using Data.Enums;
using Managers;
using PowerUp.Projectiles;
using UnityEngine;

namespace Powerup.Powerups
{
	public class RocketLauncher : PowerupBase
	{
		private void OnEnable()
		{
			ammo = 5;
		}

		protected override void Execute()
		{
			foreach (Transform hardpoint in hardpoints)
			{
				Vector3 direction = hardpoint.forward;
				
				Rocket rocket = ServiceLocator.ObjectPools.SpawnFromPool(ObjectPoolType.Rocket).GetComponent<Rocket>();
				rocket.transform.position = hardpoint.position + direction * 0.5f;
				rocket.transform.rotation = hardpoint.rotation;
				rocket.SetupProjectile(direction);
			}
		}
	}
}