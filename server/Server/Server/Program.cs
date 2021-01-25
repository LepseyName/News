using System;
using System.Collections.Generic;
using System.Threading;

namespace Server
{
    class Program
    {

        static void Main(string[] args)
        {
            List<Server> servers;
            
            GeneralData generalData = Storage.getGeneralData();
            Storage.logInitialize(generalData.logFileName);
            Storage.setBasePatchImage(generalData.basePathImage);
            ServerData serverData = Storage.getServerData(generalData.serverInitFileName);
            serverData.Headers.headersToByte();
            Storage.inLogWriter("ServerData initialize good!");

            if (DataBase.Initialize(generalData.serverDataBase, generalData.nameDataBase, generalData.loginDataBase, generalData.passwordDataBase, Storage.inLogWriter))
            {
                Storage.inLogWriter("DataBase(" + generalData.serverDataBase + "/" + generalData.nameDataBase + ") connect good!");
            }
            else
            {
                Storage.inLogWriter("DataBase(" + generalData.serverDataBase + "/" + generalData.nameDataBase + ") not connect! Fatal Error! Server stopped!");
                Storage.Close();
                return;
            }

            servers = startServers(generalData.portsUser[0], generalData.portsEditor[0], generalData.portsAdmin[0], Storage.inLogWriter, serverData);

            
            string input = "";
            while(input != "exit")
            {
                input = Console.ReadLine();
            }

            foreach (var t in servers)
            {
                t.Stop();
                t.Dispose();
            }
                
            Thread.Sleep(1000);

            Storage.Close();
        }

        static List<Server> startServers(int userServerport, int editorServerport, int adminServerport, Func<string,bool> logWriter, ServerData data)
        {
            List<Server> threads = new List<Server>(3);
            
            logWriter.Invoke("Start initialize servers...");
            logWriter.Invoke("ports: " + userServerport + ", " + editorServerport + ", " + adminServerport);
            Server user = new Server(userServerport, logWriter, data),
                 editor = new Server(editorServerport, logWriter, data),
                  admin = new Server(adminServerport, logWriter, data);
            threads.Add(user);
            threads.Add(editor);
            threads.Add(admin);

            Thread buff;
            logWriter.Invoke("Run servers...");

            buff = new Thread(new ThreadStart(user.Start));
            buff.Start();
            
            buff = new Thread(new ThreadStart(editor.Start));
            buff.Start();

            buff = new Thread(new ThreadStart(admin.Start));
            buff.Start();

            return threads;
        }
    }

    
}
