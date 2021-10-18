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
		
		public override void OnStartLocalPlayer()
		{
			inputs = GetComponent<InputManager>();
		}

		private void Update()
		{
			if (!isLocalPlayer)
				return;
			
			if (inputs.isUsingPowerup)
				CmdUse();

			if (inputs.isDroppingPowerup)
				CmdDrop();
		}

		[Command]
		public void CmdUse()
		{
			if (currentPowerupType == PowerupType.NONE)
				return;
			
			powerups[(int)currentPowerupType].Use();

			if (powerups[(int)currentPowerupType].GetAmmo() <= 0)
				Drop();
		}

		[Server]
		public void Pickup(PowerupType newPowerUp)
		{
			if (currentPowerupType != PowerupType.NONE || currentPowerupType == newPowerUp)
				return;
			
			currentPowerupType = newPowerUp;

			UpdateClientPickup(newPowerUp);
		}
		
		private void UpdateClientPickup(PowerupType newPowerupType)
		{
			if (currentPowerupType != PowerupType.NONE)
			{
				RpcDeactivateObject((int)currentPowerupType);
			}

			if (newPowerupType != PowerupType.NONE)
			{
				RpcActivateObject((int)newPowerupType);
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
			UpdateClientPickup(PowerupType.NONE);
			currentPowerupType = PowerupType.NONE;
		}

		[Command]
		private void CmdDrop()
		{
			UpdateClientPickup(PowerupType.NONE);
			currentPowerupType = PowerupType.NONE;
		}
	}
}