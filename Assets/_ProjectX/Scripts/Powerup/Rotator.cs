using UnityEngine;

namespace PowerUp
{
    public class Rotator : MonoBehaviour
    {
        [SerializeField] private Vector3 speed;

        public bool doRotate;
        
        void Update ()
        {
            if (speed == Vector3.zero || !doRotate)
                return;
            
            transform.Rotate(speed.x * Time.deltaTime, speed.y * Time.deltaTime, speed.z * Time.deltaTime, Space.Self);
        }
    }
}
