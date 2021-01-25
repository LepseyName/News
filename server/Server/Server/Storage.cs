using Org.BouncyCastle.Math.EC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace Server
{
    public static class Storage
    {
        static private FileStream logStrim;
        static private string basePathImage;
        
        static public GeneralData getGeneralData()
        {
            GeneralData data = new GeneralData();
            try
            {
                StreamReader generalFile = new StreamReader(new FileStream("init", FileMode.Open)  );
                string buffer;
                string[] ports;

                buffer = generalFile.ReadLine();
                data.basePathImage = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = generalFile.ReadLine();
                data.serverDataBase = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = generalFile.ReadLine();
                data.nameDataBase = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = generalFile.ReadLine();
                data.loginDataBase = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = generalFile.ReadLine();
                data.passwordDataBase = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = generalFile.ReadLine();
                data.generalPort = Convert.ToInt16( buffer.Substring(buffer.IndexOf('=') + 1).Trim());

                buffer = generalFile.ReadLine();
                buffer = buffer.Substring(buffer.IndexOf('=') + 1).Trim();
                ports = buffer.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                data.portsUser = new short[ports.Length];
                for (int i = 0; i < ports.Length; i++)
                    data.portsUser[i] = Convert.ToInt16(ports[i]);

                buffer = generalFile.ReadLine();
                buffer = buffer.Substring(buffer.IndexOf('=') + 1).Trim();
                ports = buffer.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                data.portsEditor = new short[ports.Length];
                for (int i = 0; i < ports.Length; i++)
                    data.portsEditor[i] = Convert.ToInt16(ports[i]);

                buffer = generalFile.ReadLine();
                buffer = buffer.Substring(buffer.IndexOf('=') + 1).Trim();
                ports = buffer.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                data.portsAdmin = new short[ports.Length];
                for (int i = 0; i < ports.Length; i++)
                    data.portsAdmin[i] = Convert.ToInt16(ports[i]);

                buffer = generalFile.ReadLine();
                data.logFileName = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = generalFile.ReadLine();
                data.serverInitFileName = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                generalFile.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Environment.Exit(0);
            }
            return data;
        }

        static public ServerData getServerData(string path)
        {
            ServerData data = new ServerData();
            try
            {
                StreamReader initFile = new StreamReader(new FileStream(path, FileMode.Open));
                string buffer;

                buffer = initFile.ReadLine();
                data.milisecondsLive = Convert.ToUInt16(buffer.Substring(buffer.IndexOf('=') + 1).Trim());

                buffer = initFile.ReadLine();
                data.maxLenMessage = Convert.ToUInt64(buffer.Substring(buffer.IndexOf('=') + 1).Trim());

                buffer = initFile.ReadLine();
                data.maxLenQueuenAltNews = Convert.ToUInt32(buffer.Substring(buffer.IndexOf('=') + 1).Trim());

                buffer = initFile.ReadLine();
                data.minPeriodComplaint = Convert.ToUInt32(buffer.Substring(buffer.IndexOf('=') + 1).Trim());


                buffer = initFile.ReadLine();
                data.maxCountThread = Convert.ToUInt16(buffer.Substring(buffer.IndexOf('=') + 1).Trim());

                buffer = initFile.ReadLine();
                data.webSocketHendshack = Encoding.ASCII.GetBytes(buffer.Substring(buffer.IndexOf('=') + 1).Trim());


                data.Headers = new ServerHeaders();
                buffer = initFile.ReadLine();
                data.Headers.EndSeans = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = initFile.ReadLine();
                data.Headers.Good = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = initFile.ReadLine();
                data.Headers.EndMessages = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = initFile.ReadLine();
                data.Headers.Separator = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = initFile.ReadLine();
                data.Headers.ReplaceSeparator = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = initFile.ReadLine();
                data.Headers.GetAllNews = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = initFile.ReadLine();
                data.Headers.GetNews = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = initFile.ReadLine();
                data.Headers.Complaint = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = initFile.ReadLine();
                data.Headers.GetImage = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = initFile.ReadLine();
                data.Headers.GetAllMyNews = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = initFile.ReadLine();
                data.Headers.AnyNews = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = initFile.ReadLine();
                data.Headers.DeleteNews = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = initFile.ReadLine();
                data.Headers.GetAllMyEditors = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = initFile.ReadLine();
                data.Headers.AnyEditors = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = initFile.ReadLine();
                data.Headers.DeleteEditor = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = initFile.ReadLine();
                data.Headers.LogIn = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = initFile.ReadLine();
                data.Headers.LogUot = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                buffer = initFile.ReadLine();
                data.Headers.Key = buffer.Substring(buffer.IndexOf('=') + 1).Trim();

                if (!data.Headers.headersToByte()) throw new Exception("Invalide ServerData file format!");

                initFile.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Environment.Exit(0);
            }
            return data;
        }

        static public bool logInitialize(string Path)
        {
            try
            {
                logStrim = new FileStream(Path, FileMode.Append, FileAccess.Write);
                Storage.inLogWriter("\n\nLog output initialize.");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string loadImage(string imageURL)
        {
            int fIndex = imageURL.IndexOf("file:///");
            if (fIndex >= 0)
            {
                imageURL = imageURL.Substring(0, fIndex) + imageURL.Substring(fIndex + "file:///".Length);
            }
            
            string[] goodExtensions = {".png", ".jpg", ".mpeg" };
            string extension = imageURL.LastIndexOf('.') > 0 ? imageURL.Substring( imageURL.LastIndexOf('.')) : "";
            for (int i=0; i<goodExtensions.Length; i++)
                if (extension == goodExtensions[i])
                {
                    try
                    {
                        string name = Tools.RandomString(10) + extension;
                        using (var client = new WebClient())
                        {
                            client.DownloadFile(imageURL, Storage.basePathImage + "/" + name);
                        }
                        return name;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                        
                }  
            return null;
        }

        static public string saveImage(string name, string data)
        {
            if(Storage.basePathImage != null && Storage.basePathImage.Length > 10)
            {
                string newName = "";
                do
                {
                    newName = Storage.basePathImage + "\\" + Tools.RandomString(10) + (name.IndexOf('.') > 0 ? name.Substring(name.IndexOf('.')) : "");
                } while (File.Exists(newName));

                try
                {
                    FileStream output = new FileStream(newName, FileMode.Create);
                    byte[] array = Convert.FromBase64String(data);
                    output.Write(array, 0, array.Length);
                    output.Close();
                    return newName.Substring(newName.LastIndexOf('\\')+1);
                }
                catch (Exception)
                {
                    return "";
                }
            }
            return "";
        }

        static public bool inLogWriter(string Message)
        {
            if (logStrim.CanWrite)
            {
                try
                {
                    Message = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + Message + "\n";
                    byte[] buff = Encoding.UTF8.GetBytes(Message);
                    Console.Write(Message);
                    logStrim.Write(buff, 0, buff.Length);
                    logStrim.Flush();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else return false;
        }

        static public void Close()
        {
            try
            {
                logStrim.Close();
            }
            catch (Exception)
            {

            }
        }

        static public void setBasePatchImage(string path)
        {
            Storage.basePathImage = path;
        }
    }
}
