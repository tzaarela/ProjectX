using System;
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
        [Header("SETTINGS:")]
        [SerializeField] private float velocityZoomMultiplier = 1;
        [SerializeField] private float maxZoomOutDistance = 60f;
        [SerializeField] private float minZoomInDistance = 40f;
        
        [Header("EXT. REFERENCES:")]
        [SerializeField] private CinemachineVirtualCamera zoomInCamera;
        [SerializeField] private CinemachineVirtualCamera flagTargetCamera;
        [SerializeField] private GameObject mapFlag;
        
        private CinemachineVirtualCamera gameCamera;
        private PlayerController playerController;

		private void Awake()
        {
            GlobalMediator.Instance.Subscribe(this);
            gameCamera = GetComponent<CinemachineVirtualCamera>();
        }

        private void Start()
        {
            flagTargetCamera.Follow = mapFlag.transform;
        }

        private void Update()
		{
			if (playerController != null)
			{
                float localForwardVelocity = Vector3.Dot(playerController.rb.velocity, playerController.transform.forward);

                float zoom = Mathf.Clamp(localForwardVelocity * velocityZoomMultiplier, minZoomInDistance, maxZoomOutDistance);
                gameCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = zoom;
            }
		}
        

        public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
        {
            switch (eventState)
            {
                case GlobalEvent.LOCAL_PLAYER_CONNECTED_TO_GAME:

                    if (globalSignalData is GameObjectData localPlayer)
                    {
                        gameCamera.Follow = localPlayer.gameObject.transform;
                        zoomInCamera.Follow = localPlayer.gameObject.transform;
                        playerController = localPlayer.gameObject.GetComponent<PlayerController>();
                    }
                    break;
                
                case GlobalEvent.FLAG_TAKEN:

                    if (globalSignalData is GameObjectData playerWithFlag)
                    {
                        flagTargetCamera.Follow = playerWithFlag.gameObject.transform;
                    }
                    break;
                
                case GlobalEvent.FLAG_DROPPED:
                    
                    flagTargetCamera.Follow = mapFlag.transform;
                    break;
            }
        }
    }
}
