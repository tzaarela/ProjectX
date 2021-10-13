using System.Collections.Generic;
using _Content.Scripts.Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using UnityEngine;

namespace Singletons
{
    [DefaultExecutionOrder(-9)]
    public class GlobalMediator : MonoBehaviour, IReceiveGlobalSignal, ISendGlobalSignal
    {
        public static GlobalMediator Instance;

        private List<IReceiveGlobalSignal> subscribers = new List<IReceiveGlobalSignal>();
        
        public int totalSubscribers;
        
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }
        
        public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData signalData)
        {
            SendGlobal(eventState, signalData);
        }

        public void SendGlobal(GlobalEvent eventState, GlobalSignalBaseData signalData)
        {
            for (int i = subscribers.Count -1; i >= 0; i--)
            {
                subscribers[i].ReceiveGlobal(eventState, signalData);
            }
        }

        public void Subscribe(IReceiveGlobalSignal subscriber)
        {
            subscribers.Add(subscriber);
            totalSubscribers++;
        }

        public void UnSubscribe(IReceiveGlobalSignal subscriber)
        {
            subscribers.Remove(subscriber);
            totalSubscribers--;
        }
    }
}
