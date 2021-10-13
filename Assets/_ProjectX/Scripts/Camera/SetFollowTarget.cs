using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Managers;
using UnityEngine;
using Cinemachine;

public class SetFollowTarget : MonoBehaviour, IReceiveGlobalSignal
{
    private CinemachineVirtualCamera virtualCamera;

    private void Start()
    {
        GlobalMediator.Instance.Subscribe(this);

        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }
    
    public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
    {
        switch (eventState)
        {
            case GlobalEvent.LOCAL_PLAYER:

                if (globalSignalData is GameObjectData data)
                {
                    virtualCamera.Follow = data.gameObject.transform;
                }
                
                break;
        }
    }
}
