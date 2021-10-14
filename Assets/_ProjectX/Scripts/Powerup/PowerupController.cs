using Data.Enums;
using Player;
using PowerUp;
using Utilites;
using UnityEngine;

public class PowerupController : MonoBehaviour
{
    [SerializeField]private PowerupType currentPowerupType;

    private Rotator rotator;

    private void Start()
    {
        rotator = GetComponent<Rotator>();
    }
    
    private void OnEnable()
    {
        currentPowerupType = EnumUtils.RandomEnumValue<PowerupType>(1);
        rotator.doRotate = true;
    }

    private void OnDisable()
    {
        rotator.doRotate = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PowerupSlot>().Pickup(currentPowerupType);
        }
    }
}
