using Data.Containers.GlobalSignal;
using UnityEngine.Timeline;

namespace Data.Containers
{
    public class CameraShakeData : GlobalSignalBaseData
    {
        public float duration;
        public float magnitude;

        public CameraShakeData(float duration, float magnitude)
        {
            this.duration = duration;
            this.magnitude = magnitude;
        }
    }
}