using FMODUnity;
using UnityEngine;


public class PlayerSound : MonoBehaviour
{
    private Rigidbody rb;
    private StudioEventEmitter emitter;
    
    public float minRPM = 0;
    public float maxRPM = 300;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
    }

    private void Start()
    {
        emitter = GetComponent<StudioEventEmitter>();
    }

    void Update()
    {
        float rpm = GetForwardVelocity() / 50;
        
        Debug.Log("RPM " + rpm);
        
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
}
