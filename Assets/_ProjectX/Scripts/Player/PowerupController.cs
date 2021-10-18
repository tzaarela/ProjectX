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
		
		[SerializeField] private PowerupType currentPowerupType;

		// Important that added items follows same pattern as PowerupType
		public List<PowerupBase> powerups = new List<PowerupBase>();
		
		public PowerupBase currentPowerup;


		public override void OnStartServer()
		{
			if (currentPowerupType != PowerupType.NONE)
				currentPowerup = powerups[(int)currentPowerupType - 1];
		}
		public override void OnStartLocalPlayer()
		{
			//if (currentPowerup != PowerupType.NONE)
			//	powerup = powerups[(int)currentPowerup - 1];

			inputs = GetComponent<InputManager>();
		}

		private void Update()
		{
			if (!isLocalPlayer)
				return;

			//if (currentPowerup == PowerupType.NONE)
			//	return;
			
			if (inputs.isUsingPowerup)
				CmdUse();

			if (inputs.isDroppingPowerup)
				CmdDrop();
		}

		[Command]
		public void CmdUse()
		{
			if(currentPowerup == null)
				return;
			
			currentPowerup.Use();

			if (currentPowerup.GetAmmo() <= 0)
				Drop();
		}

		[Server]
		public void Pickup(PowerupType newPowerUp)
		{
			if (currentPowerupType != PowerupType.NONE || currentPowerupType == newPowerUp)
				return;
			
			// -1 as PowerupType.NONE is not in list powerups
			currentPowerup = powerups[(int)newPowerUp - 1];
			
			currentPowerupType = newPowerUp;

			UpdateClientPickup(newPowerUp);
		}
		
		
		private void UpdateClientPickup(PowerupType newPowerupType)
		{
			if (currentPowerupType != PowerupType.NONE)
			{
				RpcDeactivateObject((int)currentPowerupType - 1);
				
			}

			if (newPowerupType != PowerupType.NONE)
			{
				RpcActivateObject((int)newPowerupType - 1);
			}
			
		}

		[ClientRpc]
		private void RpcActivateObject(int powerIndex)
		{
			powerups[powerIndex].gameObject.SetActive(true);
		}


		[ClientRpc]
		private void RpcDeactivateObject(int powerIndex)
		{
			powerups[powerIndex].gameObject.SetActive(false);
		}

		[Server]
		private void Drop()
		{
			currentPowerup = null;
			UpdateClientPickup(PowerupType.NONE);
			currentPowerupType = PowerupType.NONE;
		}

		[Command]
		private void CmdDrop()
		{
			currentPowerup = null;
			UpdateClientPickup(PowerupType.NONE);
			currentPowerupType = PowerupType.NONE;
		}
	}
}