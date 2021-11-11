using Cinemachine;
using Data.Containers;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Managers;
using UnityEngine;

namespace Cameras
{
    public class CinemachineShake : MonoBehaviour, IReceiveGlobalSignal
    {
        private CinemachineVirtualCamera cinemachineVirtualCamera;
        private float shakeTimer;
        private float shakeTimerTotal;
        private float startingIntensity;

        private void Awake() 
        {
            cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
        }

        private void Start()
        {
            GlobalMediator.Instance.Subscribe(this);
        }

        private void ShakeCamera(float intensity, float time) 
        {
            CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = 
                cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;

            startingIntensity = intensity;
            shakeTimerTotal = time;
            shakeTimer = time;
        }

        private void Update() 
        {
            if (shakeTimer > 0) {
                shakeTimer -= Time.deltaTime;
                CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
                    cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

                cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 
                    Mathf.Lerp(startingIntensity, 0f, 1 - shakeTimer / shakeTimerTotal);
            }
        }
        
        public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
        {
            if (eventState == GlobalEvent.CAMERA_SHAKE)
            {
                if (globalSignalData is CameraShakeData data)
                {
                    float distance = 45f - data.distance;

                    distance = distance / 10f;
				
                    if (distance < 0f)
                        distance = 0f;

                    float intensity = data.intensity;

                    intensity *= distance;
                    
                    ShakeCamera(intensity, data.duration);
                }
            }
        }
    }
}
