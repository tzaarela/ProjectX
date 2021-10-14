using Cinemachine;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Managers;
using UnityEngine;

namespace Camera
{
    public class SetFollowTarget : MonoBehaviour, IReceiveGlobalSignal
    {
        private CinemachineVirtualCamera virtualCamera;

        private void Awake()
        {
            GlobalMediator.Instance.Subscribe(this);

            virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }
    
        public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
        {
            switch (eventState)
            {
                case GlobalEvent.SET_FOLLOW_TARGET:

                    if (globalSignalData is GameObjectData data)
                    {
                        virtualCamera.Follow = data.gameObject.transform;
                    }
                
                    break;
            }
        }
    }
}
