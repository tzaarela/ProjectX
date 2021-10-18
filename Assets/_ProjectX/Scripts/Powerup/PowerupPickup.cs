using Data.Enums;
using Player;
using PowerUp;
using Utilites;
using UnityEngine;
using Mirror;

public class PowerupPickup : NetworkBehaviour
{
    [SerializeField] private PowerupType currentPowerupType;

    private Rotator rotator;

    private void Awake()
    {
        rotator = GetComponent<Rotator>();
    }
    
    private void OnEnable()
    {
        rotator.doRotate = true;
        currentPowerupType = EnumUtils.RandomEnumValue<PowerupType>(1);
    }

    private void OnDisable()
    {
        rotator.doRotate = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer)
            return;
        
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PowerupSlot>().Pickup(currentPowerupType);
            NetworkServer.Destroy(gameObject);
        }
    }
}
