using Data.Enums;
using Managers;
using Powerup.Projectiles;
using PowerUp.Projectiles;
using UnityEngine;

namespace Powerup.Powerups
{
	public class MineDeployer : PowerupBase
	{
		[SerializeField] private LayerMask groundLayer;

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
			  	RaycastHit raycastHit;
				Physics.Raycast(hardpoint.position, Vector3.down, out raycastHit, 100f, groundLayer);

				Vector3 spawnPosition = new Vector3(hardpoint.position.x, hardpoint.position.y - raycastHit.distance, hardpoint.position.z);

				Mine mine = ServiceLocator.ObjectPools.
					SpawnFromPoolWithNetId(ObjectPoolType.Mine, spawnPosition, Quaternion.identity, netID).GetComponent<Mine>();

				ammo--;

				ServiceLocator.HudManager.TargetUpdateAmmoUi(playerController.connectionToClient, ammo);
			}
		}
	}
}