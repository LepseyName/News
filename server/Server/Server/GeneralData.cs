using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public struct GeneralData
    {
        //database data
        public string serverDataBase;
        public string nameDataBase;
        public string loginDataBase;
        public string passwordDataBase;
        //end

        //server data
        public short generalPort;
        public short[] portsUser;
        public short[] portsEditor;
        public short[] portsAdmin;

        public string serverInitFileName;
        //end

        public string basePathImage;
        public string logFileName;
    };
}
