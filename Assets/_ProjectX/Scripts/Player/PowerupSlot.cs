using Data.Enums;
using System.Collections.Generic;
using Powerup.Powerups;
using UnityEngine;

namespace Player
{
	public class PowerupSlot : MonoBehaviour
	{
		private InputManager inputs;
		
		[SerializeField]private PowerupType currentPowerup;

		// Important that added items follows same pattern as PowerupType
		public List<PowerupBase> powerups = new List<PowerupBase>();
		
		public PowerupBase powerup;

		private void Start()
		{
			inputs = GetComponent<InputManager>();
		}

		private void Update()
		{
			if (currentPowerup == PowerupType.NONE)
				return;
			
			if (inputs.isUsingPowerup)
			{
				powerup.Use();
			}

			if (powerup.GetAmmo() <= 0)
			{
				Drop();
			}
		}

		public void Pickup(PowerupType newPowerUp)
		{
			if (currentPowerup != PowerupType.NONE || currentPowerup == newPowerUp)
				return;
			
			// -1 as PowerupType.NONE is not in list powerups
			powerup = powerups[(int)newPowerUp - 1];
			
			currentPowerup = newPowerUp;
		}

		private void Drop()
		{
			currentPowerup = PowerupType.NONE;
			powerup = null;
		}
	}
}