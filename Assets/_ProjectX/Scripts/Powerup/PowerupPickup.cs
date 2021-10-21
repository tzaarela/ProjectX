using Data.Enums;
using Player;
using PowerUp;
using Utilites;
using UnityEngine;
using Mirror;

public class PowerupPickup : NetworkBehaviour
{
    [SerializeField] private PowerupType currentPowerupType;
    
    public GameObject body;
    private Collider coll;
    private Rotator rotator;

    public float disableTimestamp;
    
    private void Awake()
    {
        coll = GetComponent<Collider>();
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
            disableTimestamp = Time.time;
            RpcDisableObject();
        }
    }
    
    [ClientRpc]
    private void RpcDisableObject()
    {
        coll.enabled = false;
        rotator.doRotate = false;
        body.SetActive(false);
    }
    
    [ClientRpc]
    public void RpcEnableObject()
    {
        coll.enabled = true;
        rotator.doRotate = true;
        body.SetActive(true);
        currentPowerupType = EnumUtils.RandomEnumValue<PowerupType>(0);
    }
}
