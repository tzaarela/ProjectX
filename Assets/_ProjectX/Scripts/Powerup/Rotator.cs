using UnityEngine;

namespace PowerUp
{
    public class Rotator : MonoBehaviour
    {
        [SerializeField] private Vector3 normalSpeed;
        [SerializeField] private Vector3 slowSpeed;

        private Vector3 currentSpeed = Vector3.zero;
        
        public bool doRotate;
        
        void Update ()
        {
            if (normalSpeed == Vector3.zero || !doRotate)
                return;
            
            transform.Rotate(currentSpeed.x * Time.deltaTime, currentSpeed.y * Time.deltaTime, currentSpeed.z * Time.deltaTime, Space.Self);
        }

        public void SetSpeedSlow()
        {
            currentSpeed = slowSpeed;
        }

        public void SetSpeedNormal()
        {
            currentSpeed = normalSpeed;
        }
    }
}
