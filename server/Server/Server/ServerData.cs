using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public struct ServerData
    {
        public ServerHeaders Headers;

        public uint milisecondsLive;
        public ulong maxLenMessage;
        public uint maxLenQueuenAltNews;
        public uint minPeriodComplaint;
        public ushort maxCountThread;
        public byte[] webSocketHendshack;

    }
}
