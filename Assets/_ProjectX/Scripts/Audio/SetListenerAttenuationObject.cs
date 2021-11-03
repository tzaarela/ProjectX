using System;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Managers;
using UnityEngine;

namespace Audio
{
	public class SetListenerAttenuationObject : MonoBehaviour, IReceiveGlobalSignal
	{
		private FMODUnity.StudioListener listener;

		private void Start()
		{
			listener = GetComponent<FMODUnity.StudioListener>();
			
			GlobalMediator.Instance.Subscribe(this);
		}

		public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			switch (eventState)
			{
				case GlobalEvent.LOCAL_PLAYER_CONNECTED_TO_GAME:
				{
					if (globalSignalData is GameObjectData data)
					{
						listener.attenuationObject = data.gameObject;
					}
                
					break;
				}

				case GlobalEvent.SET_FOLLOW_TARGET:
				{
					if (globalSignalData is GameObjectData data)
					{
						listener.attenuationObject = data.gameObject;
					}
                
					break;
				}
			}
		}
	}
}