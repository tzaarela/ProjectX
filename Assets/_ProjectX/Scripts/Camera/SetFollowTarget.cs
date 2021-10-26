using Cinemachine;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Managers;
using UnityEngine;

namespace Cameras
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
                case GlobalEvent.LOCAL_PLAYER_CONNECTED_TO_GAME:

                    if (globalSignalData is GameObjectData localPlayer)
                    {
                        virtualCamera.Follow = localPlayer.gameObject.transform;
                    }
                
                    break;
                
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
