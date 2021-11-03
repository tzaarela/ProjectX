using Data.Enums;
using System.Collections.Generic;
using Managers;
using Mirror;
using Powerup.Powerups;
using UnityEngine;

namespace Player
{
	public class PowerupController : NetworkBehaviour
	{
		private InputManager inputs;
		private NetworkIdentity networkIdentity;

		[SerializeField] private PowerupType currentPowerupType;

		// Important that added items follows same pattern as PowerupType
		public List<PowerupBase> powerups = new List<PowerupBase>();
		
		public override void OnStartLocalPlayer()
		{
			inputs = GetComponent<InputManager>();
			networkIdentity = GetComponent<NetworkIdentity>();
		}

		private void Update()
		{
			if (!isLocalPlayer)
				return;
			
			if (inputs.isUsingPowerup)
				CmdUse((int)networkIdentity.netId);

			if (inputs.isDroppingPowerup)
				CmdDrop();
		}

		[Command]
		public void CmdUse(int netID)
		{
			if (currentPowerupType == PowerupType.NONE)
				return;
			
			powerups[(int)currentPowerupType].Use(netID);

			if (powerups[(int)currentPowerupType].GetAmmo() <= 0)
				Drop();
		}

		[Server]
		public bool Pickup(PowerupType newPowerUp)
		{
			if (currentPowerupType != PowerupType.NONE || currentPowerupType == newPowerUp)
				return false;
			
			currentPowerupType = newPowerUp;

			UpdateClientPickup(newPowerUp);

			return true;
		}
		
		private void UpdateClientPickup(PowerupType newPowerupType)
		{
			if (currentPowerupType != PowerupType.NONE)
			{
				RpcDeactivateObject((int)currentPowerupType);
			}

			if (newPowerupType != PowerupType.NONE)
			{
				RpcActivateObject((int)currentPowerupType);
			}
		}

		[ClientRpc]
		private void RpcActivateObject(int powerIndex)
		{
			powerups[powerIndex].gameObject.SetActive(true);
			
			if (!isLocalPlayer)
				return;
			
			int startingAmmo = powerups[powerIndex].gameObject.GetComponent<PowerupBase>().GetAmmo();
			ServiceLocator.HudManager.ActivatePowerupUi(powerIndex, startingAmmo);
		}
		
		[ClientRpc]
		private void RpcDeactivateObject(int powerIndex)
		{
			powerups[powerIndex].gameObject.SetActive(false);
			
			if (!isLocalPlayer)
				return;
			
			ServiceLocator.HudManager.DeactivatePowerupUi();
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