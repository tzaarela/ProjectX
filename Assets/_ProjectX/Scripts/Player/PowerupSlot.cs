using Data.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
	public class PowerupSlot : MonoBehaviour
	{
		[SerializeField]private PowerupType currentPowerup;

		public List<PowerupBase> powerups = new List<PowerupBase>();
		
		public bool hasPowerup;
		public PowerupBase powerup;

		public void Pickup(PowerupType newPowerUp)
		{
			if (currentPowerup != PowerupType.NONE)
				return;

			hasPowerup = true;
			currentPowerup = newPowerUp;
		}

		private void Drop()
		{
			currentPowerup = PowerupType.NONE;
			hasPowerup = false; 
		}
	}
}