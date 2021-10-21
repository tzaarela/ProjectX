using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Managers
{
    public class PowerupManager : NetworkBehaviour
    {
        private List<PowerupPickup> powerupPickups = new List<PowerupPickup>();

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

        private void Update()
        {
            if (nextRespawn > Time.time)
                return;
			
            nextRespawn = respawnCooldown + Time.time;
        }
    }
}
