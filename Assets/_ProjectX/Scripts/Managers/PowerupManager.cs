using System.Collections.Generic;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Mirror;
using UnityEngine;
using Utilites;

namespace Managers
{
    public class PowerupManager : NetworkBehaviour, IReceiveGlobalSignal
    {
        private List<PowerupPickup> powerupPickups = new List<PowerupPickup>();

        [SerializeField] float respawnPowerupTime;
        [SerializeField] float respawnCooldown;
        private float nextRespawn = 0;

        private bool allPlayersConnected;

        private void Start()
        {
            GlobalMediator.Instance.Subscribe(this);
            
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

            if (!allPlayersConnected)
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

        public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
        {
            if (eventState == GlobalEvent.ALL_PLAYERS_CONNECTED_TO_GAME)
            {
                allPlayersConnected = true;
            }
        }
    }
}
