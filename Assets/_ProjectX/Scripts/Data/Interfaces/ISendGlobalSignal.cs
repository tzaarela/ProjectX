using System;
using Data.Containers.GlobalSignal;
using Data.Enums;

namespace Data.Interfaces
{
    public interface ISendGlobalSignal
    {
        public void SendGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null);
    }
}