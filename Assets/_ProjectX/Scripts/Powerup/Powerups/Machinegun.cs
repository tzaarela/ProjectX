using Data.Enums;
using Managers;
using Mirror;
using PowerUp.Projectiles;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Powerup.Powerups
{
	public class Machinegun : PowerupBase
	{
		[SerializeField] private float maxSpreadOffset = 3.5f;
		
		protected override void Start()
		{
			base.Start();

			forwardSpawnOffset = 0.0f;
			heightSpawnOffset = -0.5f;
			fireCooldown = 0.13f;
		}

		private void OnEnable()
		{
			ammo = 60;
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
					
					Vector3 eulerAngles = hardpoint.transform.eulerAngles;
					float spread = Random.Range(-maxSpreadOffset, maxSpreadOffset);
					Random.InitState(Random.Range(0, 500));
					float angle = eulerAngles.y;
					Quaternion bulletSpreadRotation = Quaternion.Euler(new Vector3(eulerAngles.x,angle + spread,eulerAngles.z));

					Bullet bullet = ServiceLocator.ObjectPools.SpawnFromPoolWithNetId(ObjectPoolType.Bullet, hardpoint.position + direction, bulletSpreadRotation, netID).GetComponent<Bullet>();

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