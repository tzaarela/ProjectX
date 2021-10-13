using UnityEngine;

namespace _Content.Scripts.Data.Containers.GlobalSignal
{
    public class TransformData : GlobalSignalBaseData
    {
        public Vector3 position;
        public Vector3 angle;
        public Vector3 scale;

        public TransformData()
        {
            
        }

        public TransformData(Vector3 newPosition)
        {
            position = newPosition;
        }
        
        public TransformData(Vector3 newPosition, Vector3 newAngle)
        {
            position = newPosition;
            angle = newAngle;
        }

        public TransformData(Vector3 newPosition, Vector3 newAngle, Vector3 newScale)
        {
            position = newPosition;
            angle = newAngle;
            scale = newScale;
        }
    }
}