using Data.Enums;
using Player;
using PowerUp;
using Utilites;
using UnityEngine;
using Mirror;
using System;

public class PowerupPickup : NetworkBehaviour
{
    [SerializeField] private PowerupType currentPowerupType;

    private Rotator rotator;

    private void Awake()
    {
        rotator = GetComponent<Rotator>();
    }

	private void Start()
	{
        rotator.doRotate = true;
	}

	private void OnEnable()
    {
        currentPowerupType = EnumUtils.RandomEnumValue<PowerupType>(0);
    }

    [Server]
    private void OnTriggerEnter(Collider other)
    {
        PickUp(other.gameObject);
    }

	private void PickUp(GameObject playerObj)
	{
        if (playerObj.CompareTag("Player"))
        {
            Debug.Log("CURRENT PICKUP: " + currentPowerupType);
            playerObj.GetComponent<PowerupController>().Pickup(currentPowerupType);
            NetworkServer.Destroy(gameObject);
        }
    }
}
