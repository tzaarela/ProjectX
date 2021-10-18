using Data.Enums;
using System.Collections.Generic;
using Mirror;
using Powerup.Powerups;
using UnityEngine;

namespace Player
{
	public class PowerupController : NetworkBehaviour
	{
		private InputManager inputs;
		
		[SerializeField]private PowerupType currentPowerup;

		// Important that added items follows same pattern as PowerupType
		public List<PowerupBase> powerups = new List<PowerupBase>();
		
		public PowerupBase powerup;


		public override void OnStartServer()
		{
			if (currentPowerup != PowerupType.NONE)
				powerup = powerups[(int)currentPowerup - 1];
		}
		public override void OnStartLocalPlayer()
		{
			if (currentPowerup != PowerupType.NONE)
				powerup = powerups[(int)currentPowerup - 1];

			inputs = GetComponent<InputManager>();
		}

		private void Update()
		{
			if (!isLocalPlayer)
				return;

			if (currentPowerup == PowerupType.NONE)
				return;
			
			if (inputs.isUsingPowerup)
			{
				CmdUse();
			}

			if (inputs.isDroppingPowerup)
			{
				Drop();
			}
			
			// if (powerup.GetAmmo() <= 0)
			// {
			// 	Drop();
			// }
		}

		[Command]
		public void CmdUse()
		{
			if(currentPowerup != PowerupType.NONE)
				powerup.Use();
		}

		[Server]
		public void Pickup(PowerupType newPowerUp)
		{
			if (currentPowerup != PowerupType.NONE || currentPowerup == newPowerUp)
				return;
			
			// -1 as PowerupType.NONE is not in list powerups
			powerup = powerups[(int)newPowerUp - 1];
			
			currentPowerup = newPowerUp;

			RpcUpdateClientPickup(newPowerUp);
		}

		[ClientRpc]
		private void RpcUpdateClientPickup(PowerupType newPowerupType)
		{
			if (currentPowerup != PowerupType.NONE)
				powerups[(int)currentPowerup - 1].gameObject.SetActive(false);
			
			if(newPowerupType != PowerupType.NONE)
				powerups[(int)newPowerupType - 1].gameObject.SetActive(true);
			
			currentPowerup = newPowerupType;
		}

		private void Drop()
		{
			CmdDrop();
		}

		[Command]
		private void CmdDrop()
		{
			powerup = null;
			RpcUpdateClientPickup(PowerupType.NONE);
		}
	}
}