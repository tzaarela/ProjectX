using Player;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    private Rigidbody rb;
    private DriveController driveController;
    private FMODUnity.StudioEventEmitter emitter;
    
    [SerializeField]
    [FMODUnity.EventRef]
    private string driftSound;
    
    private FMOD.Studio.EventInstance driftSoundInstance;
    
    public float minRPM = 0;
    public float maxRPM = 300;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        driftSoundInstance = FMODUnity.RuntimeManager.CreateInstance(driftSound);
        //FMODUnity.RuntimeManager.AttachInstanceToGameObject(driftSoundInstance, transform);
    }

    private void Start()
    {
        emitter = GetComponent<FMODUnity.StudioEventEmitter>();
    }

    void Update()
    {
        float rpm = GetForwardVelocity() / 50;
        
        float effectiveRPM = Mathf.Lerp(minRPM, maxRPM, rpm);
        emitter.SetParameter("RPM", effectiveRPM);
    }

    private float GetForwardVelocity()
    {
        float dot = Vector3.Dot(transform.forward, rb.velocity);
        
        if(Mathf.Abs(dot) > 0.1f)
        {
            float speed = rb.velocity.magnitude;
            
            return dot < 0 ? -speed : speed;
        }
        
        return 0f;
    }

    public void PlayDriftSound()
    {
        Debug.Log("Start to drift");
        driftSoundInstance.start();
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(driftSoundInstance, transform);
    }
    
    public void StopDriftSound()
    {
        Debug.Log("Stop drift");
        driftSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
    
    public void PlayEmitter()
    {
        emitter.Play();
    }

    public void StopEmitter()
    {
        emitter.Stop();
    }
}
