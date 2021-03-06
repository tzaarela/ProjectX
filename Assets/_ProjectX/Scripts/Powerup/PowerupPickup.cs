using System.Collections.Generic;
using Data.Enums;
using Player;
using PowerUp;
using UnityEngine;
using Mirror;

public class PowerupPickup : NetworkBehaviour
{
    [SerializeField] private PowerupType currentPowerupType;
    
    public GameObject body;
    private Collider coll;
    private Rotator rotator;


    public MeshRenderer meshRenderer;
    public List<Material> powerupTypeMaterials;

    [SerializeField]
    [FMODUnity.EventRef]
    private string pickupSound;

    [SerializeField] private ParticleSystem pickupParticles;

    private FMOD.Studio.EventInstance pickupSoundInstance;
    
    public float disableTimestamp;
    
    private void Awake()
    {
        coll = GetComponent<Collider>();
        rotator = GetComponentInChildren<Rotator>();
        
        pickupSoundInstance = FMODUnity.RuntimeManager.CreateInstance(pickupSound);
        pickupSoundInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
    }

	private void Start()
	{
        rotator.doRotate = true;
	}

	// private void OnEnable()
 //    {
 //        PickPowerupType();
 //    }
    
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        PickUp(other.gameObject);
    }

	private void PickUp(GameObject playerObj)
	{
        if (playerObj.CompareTag("Player"))
        {
            bool result = playerObj.GetComponent<PowerupController>().Pickup(currentPowerupType);

            if (result)
            {
                //Debug.Log("CURRENT PICKUP: " + currentPowerupType);
                disableTimestamp = Time.time;
                RpcDisableObject();
            }
        }
    }

    //TODO REMOVE IF EVERYTHING WORKS
    // private void PickPowerupType()
    // {
    //     currentPowerupType = EnumUtils.RandomEnumValue<PowerupType>(0);
    //     meshRenderer.material = powerupTypeMaterials[(int)currentPowerupType];
    // }
    
    [ClientRpc]
    private void RpcDisableObject()
    {
        pickupSoundInstance.start();
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(pickupSoundInstance, transform);
        
        pickupParticles.Play();
        
        coll.enabled = false;
        //rotator.doRotate = false;
        //body.SetActive(false);
        //rotator.SetSpeedSlow();
        currentPowerupType = PowerupType.NONE;
        meshRenderer.material = powerupTypeMaterials[powerupTypeMaterials.Count-1];
    }
    
    [ClientRpc]
    public void RpcEnableObject(PowerupType newType)
    {
        coll.enabled = true;
        rotator.doRotate = true;
        rotator.SetSpeedNormal();
        //body.SetActive(true);
        currentPowerupType = newType;
        meshRenderer.material = powerupTypeMaterials[(int)currentPowerupType];
    }
}
