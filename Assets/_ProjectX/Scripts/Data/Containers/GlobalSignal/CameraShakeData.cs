using Data.Containers.GlobalSignal;
using UnityEngine.Timeline;

namespace Data.Containers
{
    public class CameraShakeData : GlobalSignalBaseData
    {
        public float intensity;
        public float duration;
        public float distance;

        public CameraShakeData(float intensity, float duration, float distance)
        {
            this.intensity = intensity;
            this.duration = duration;
            this.distance = distance;
        }
    }
}