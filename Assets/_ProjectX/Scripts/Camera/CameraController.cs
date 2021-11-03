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
        [SerializeField] private float velocityZoomMultiplier = 1;
        [SerializeField] private float maxZoomOutDistance = 60f;
        [SerializeField] private float minZoomInDistance = 40f;

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

                float zoom = Mathf.Clamp(localForwardVelocity * velocityZoomMultiplier, minZoomInDistance, maxZoomOutDistance);
                virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = zoom;
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
