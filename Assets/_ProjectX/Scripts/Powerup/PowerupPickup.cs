using Data.Enums;
using Player;
using PowerUp;
using Utilites;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.PlayerLoop;

public class PowerupPickup : NetworkBehaviour
{
    [SerializeField] private PowerupType currentPowerupType;

    private bool sendStatus;
    private float sendDelay = 0;

    // private GameObject
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
    private void Update()
    {
        if (sendStatus)
        {
            if(sendDelay >= 5f)
                Debug.Log("GAMEOBJECT ACTIVE " + gameObject.activeSelf);
            else
            {
                sendDelay += Time.deltaTime;
                Debug.Log("SendDelay " + sendDelay);
            }
        }
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
            Debug.Log("SEND STATUS SET TO TRUE");
            RpcDisableObject();
            sendStatus = true;
        }
    }
    
    [ClientRpc]
    private void RpcDisableObject()
    {
        gameObject.SetActive(false);
    }
}
