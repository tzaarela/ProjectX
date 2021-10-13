using System;
using Data.Containers.GlobalSignal;
using Data.Enums;

namespace Data.Interfaces
{
    public interface IReceiveGlobalSignal
    {
        public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null);
    }
}