using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Managers
{
    public class PowerupManager : NetworkBehaviour
    {
        private List<PowerupPickup> powerupPickups = new List<PowerupPickup>();

        [SerializeField] float respawnPowerupTime;
        [SerializeField] float respawnCooldown;
        private float nextRespawn = 0;

        private void Awake()
        {
            GameObject[] respawns;
            respawns = GameObject.FindGameObjectsWithTag("Powerup");

            foreach (GameObject powerup in respawns)
            {
                powerupPickups.Add(powerup.GetComponent<PowerupPickup>());
            }
        }

        [Server]
        private void Update()
        {
            if (nextRespawn > Time.time)
                return;
			
            nextRespawn = respawnCooldown + Time.time;

            foreach (PowerupPickup powerupPickup in powerupPickups)
            {
                if (powerupPickup.disableTimestamp >= 0)
                {
                    if (Time.time > powerupPickup.disableTimestamp + respawnPowerupTime)
                    {
                        powerupPickup.RpcEnableObject();
                        powerupPickup.disableTimestamp = -1;
                    }
                }
            }
        }
    }
}
