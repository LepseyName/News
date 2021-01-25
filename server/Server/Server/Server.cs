using Org.BouncyCastle.Utilities.Net;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Server
{
    class Server
    {
        public static ServerData Data; 
        private int port;
        private volatile TcpListener listner;


        private List<Client> mineClient;
        private volatile bool isWork;

        private Func<string, bool> logWriter;

        private QueueDictionary<string, long> queueNews;

        public Server(int Port, Func<string, bool> logWriteFunc, ServerData serverData)
        {
            Data = serverData;
            port = Port;
            mineClient = new List<Client>(Data.maxCountThread);  
            listner = new TcpListener(System.Net.IPAddress.Any, port);
            logWriter = logWriteFunc;
            queueNews = new QueueDictionary<string, long>(serverData.maxLenQueuenAltNews); 
        }

        public void Start()
        {
            isWork = true;
            listner.Start();
            logWriter.Invoke("Start Server on port: " + port);
            while (isWork)
            {
                try{ this.newConnection(listner.AcceptTcpClient()); }
                catch(Exception e) { this.onError(e); }
            }
            logWriter.Invoke("Stop  Server on port: " + port);
        }

        ~Server()
        {
            if (listner != null)
                listner.Stop();
            logWriter.Invoke("StopListner Server on port: " + port);
            for (int i = 0; i < mineClient.Count; i++)
                mineClient[i].Stop();
            logWriter.Invoke("StopAllClient Server on port: " + port);
        }

        public void Dispose()
        {
            if (listner != null)
                listner.Stop();
            logWriter.Invoke("StopListner Server on port: " + port);
            while(mineClient.Count > 0)
            {
                mineClient[0].Stop();
                mineClient[0].Dispose();
            }
                
            logWriter.Invoke("StopAllClient Server on port: " + port);
        }

        private void newConnection(TcpClient client)
        {
            //client.
            
            if (mineClient.Count >= Data.maxCountThread)
                for (int i = 0; i < mineClient.Count; i++)
                    if (!mineClient[i].isGood())
                    {
                        mineClient.RemoveAt(i);
                        break;
                    }

            while(mineClient.Count >= Data.maxCountThread)
            {
                //Wait...
            }
            Thread t = new Thread(new ParameterizedThreadStart(StartClient));
            Client c = new Client((TcpClient)client, Data, logWriter, newDisConnection, queueNews);
            t.Start(c);
            mineClient.Add(c);
        }

        private void StartClient(object client)
        {
            Client c = (Client)client;
            c.Start();
        }

        private bool newDisConnection(Client client)
        {
            int i = mineClient.IndexOf(client);
            if (i >= 0)
                mineClient.Remove(client);
            return true;
        }

        public void Stop()
        {
            this.isWork = false;
        }

        private void onError(Exception e)
        {
            if(e.GetType() == typeof(SocketException) && isWork == true)
            {
                logWriter("Server on port: " + port + " - SocketException: " + e.Message);
            }

        }

    }
}
