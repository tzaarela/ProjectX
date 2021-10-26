using Data.Enums;
using Managers;
using PowerUp.Projectiles;
using UnityEngine;

namespace Powerup.Powerups
{
	public class RocketLauncher : PowerupBase
	{
		protected override void Start()
		{
			base.Start();

			forwardSpawnOffset = 1f;
			fireCooldown = 1.2f;
		}

		private void OnEnable()
		{
			ammo = 5;
		}

		protected override void Execute(int netID)
		{
			base.Execute(netID);
			
			if (ammo > 0)
			{
				foreach (Transform hardpoint in hardpoints)
				{
					Vector3 direction = hardpoint.forward;
					
					Rocket rocket = ServiceLocator.ObjectPools.SpawnFromPool(ObjectPoolType.Rocket).GetComponent<Rocket>();
					
					if(rocket == null)
						continue;
					
					rocket.transform.position = hardpoint.position + direction * forwardSpawnOffset;
					rocket.transform.rotation = hardpoint.rotation;
					rocket.SetupProjectile(direction, netID);

					ammo--;
				}
			}
		}
	}
}