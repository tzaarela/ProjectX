using Cinemachine;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Managers;
using UnityEngine;
using Player;

namespace Cameras
{
    public class CameraController : MonoBehaviour, IReceiveGlobalSignal
    {
        private CinemachineVirtualCamera virtualCamera;
        private PlayerController playerController;

        private void Awake()
        {
            GlobalMediator.Instance.Subscribe(this);
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }

		private void Update()
		{
			if (playerController != null)
			{
                float localForwardVelocity = Vector3.Dot(playerController.rb.velocity, playerController.transform.forward);
				print(localForwardVelocity);
                virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = Mathf.Clamp(localForwardVelocity, 40, 60);
			}
		}

		public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
        {
            switch (eventState)
            {
                case GlobalEvent.LOCAL_PLAYER_CONNECTED_TO_GAME:

                    if (globalSignalData is GameObjectData localPlayer)
                    {
                        virtualCamera.Follow = localPlayer.gameObject.transform;
                        playerController = localPlayer.gameObject.GetComponent<PlayerController>();
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
