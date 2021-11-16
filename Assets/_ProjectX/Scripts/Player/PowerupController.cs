using System;
using Data.Enums;
using System.Collections.Generic;
using FMOD.Studio;
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
		private Rigidbody rb;
		
		[SerializeField] private PowerupType currentPowerupType;

		// Important that added items follows same pattern as PowerupType
		public List<PowerupBase> powerups = new List<PowerupBase>();
		
		[SerializeField]
		[FMODUnity.EventRef]
		private string dropSound;
		private FMOD.Studio.EventInstance dropSoundInstance;

		private void Start()
		{
			rb = GetComponent<Rigidbody>();
			
			dropSoundInstance = FMODUnity.RuntimeManager.CreateInstance(dropSound);
			dropSoundInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject, rb));
		}

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
			{
				CmdUse((int)networkIdentity.netId);
				
				if (currentPowerupType == PowerupType.ROCKETLAUNCHER)
				{
					powerups[(int)currentPowerupType].LocalUse();
				}
			}

			if (inputs.isDroppingPowerup)
			{
				CmdDrop();
			}
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
		public void Drop()
		{
			UpdateClientPickup(PowerupType.NONE);
			currentPowerupType = PowerupType.NONE;
		}

		[Command]
		private void CmdDrop()
		{
			if (currentPowerupType == PowerupType.NONE)
				return;

			TargetPlayDropAudio();
			UpdateClientPickup(PowerupType.NONE);
			currentPowerupType = PowerupType.NONE;

		}

		[TargetRpc]
		private void TargetPlayDropAudio()
		{
			dropSoundInstance.stop(STOP_MODE.IMMEDIATE);
			dropSoundInstance.start();
			FMODUnity.RuntimeManager.AttachInstanceToGameObject(dropSoundInstance, transform);
		}
	}
}