using System.Collections;
using Data.Containers;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Managers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Cameras
{
    public class CameraShake : MonoBehaviour, IReceiveGlobalSignal
    {
        private void Start()
        {
            GlobalMediator.Instance.Subscribe(this);
        }

        IEnumerator ShakeCamera(float duration, float magnitude)
        {
            Vector3 originalPosition = transform.localPosition;

            Debug.Log("Original Position " + originalPosition);
        
            float elapsed = 0.0f;

            while (elapsed < duration)
            {
                float shakeX = Random.Range(-1f, 1f) * magnitude;
                float shakeY = Random.Range(-1f, 1f) * magnitude;

                transform.localPosition = new Vector3(shakeX + originalPosition.x, shakeY + originalPosition.y, originalPosition.z);

                elapsed += Time.deltaTime;

                yield return null;
            }

            transform.localPosition = originalPosition;
        }
    
        public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
        {
            if (eventState == GlobalEvent.CAMERA_SHAKE)
            {
                if (globalSignalData is CameraShakeData data)
                {
                    StartCoroutine(ShakeCamera(data.duration, data.magnitude));
                }
            }
        }
    }
}
