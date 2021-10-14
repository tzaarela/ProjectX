using Data.Enums;
using UnityEngine;

namespace Player
{
    public class PlayerPowerup : MonoBehaviour
    {
         [SerializeField]private PowerupType currentPowerup;

         public void Pickup(PowerupType newPowerUp)
         {
             if (currentPowerup != PowerupType.NONE)
                 return;

             Debug.Log("Picked up: " + newPowerUp, gameObject);
             
             currentPowerup = newPowerUp;
         }

         
         private void Drop()
         {
             currentPowerup = PowerupType.NONE;
         }
    }
}