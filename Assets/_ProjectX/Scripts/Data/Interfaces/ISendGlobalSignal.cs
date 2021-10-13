using System;
using _Content.Scripts.Data.Containers.GlobalSignal;
using Data.Enums;

namespace Data.Interfaces
{
    public interface ISendGlobalSignal
    {
        public void SendGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null);
    }
}