using Data.Enums;
using UnityEngine;

namespace Player
{
	public class PowerupSlot : MonoBehaviour
	{
		[SerializeField]private PowerupType currentPowerup;
		
		public bool hasPowerup;

		public void Pickup(PowerupType newPowerUp)
		{
			if (currentPowerup == PowerupType.NONE)
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