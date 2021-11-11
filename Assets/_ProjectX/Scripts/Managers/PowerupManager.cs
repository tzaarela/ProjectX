using System.Collections.Generic;
using Data.Enums;
using Mirror;
using UnityEngine;
using Utilites;

namespace Managers
{
    public class PowerupManager : NetworkBehaviour
    {
        private List<PowerupPickup> powerupPickups = new List<PowerupPickup>();

        [SerializeField] float respawnPowerupTime;
        [SerializeField] float respawnCooldown;
        private float nextRespawn = 0;

        private void Start()
        {
            GameObject[] respawns;
            respawns = GameObject.FindGameObjectsWithTag("Powerup");

            foreach (GameObject powerup in respawns)
            {
                powerupPickups.Add(powerup.GetComponent<PowerupPickup>());
            }
        }

        // [Server]
        private void Update()
        {
            if (!isServer)
                return;

            if (nextRespawn > Time.time)
                return;
			
            nextRespawn = respawnCooldown + Time.time;

            foreach (PowerupPickup powerupPickup in powerupPickups)
            {
                if (powerupPickup.disableTimestamp >= 0)
                {
                    if (Time.time > powerupPickup.disableTimestamp + respawnPowerupTime)
                    {
                        powerupPickup.RpcEnableObject(PickPowerupType());
                        powerupPickup.disableTimestamp = -1;
                    }
                }
            }
        }
        
        private PowerupType PickPowerupType()
        {
            return EnumUtils.RandomEnumValue<PowerupType>(0);
        }
    }
}
