using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Server
{
    class Client
    {
        private ServerData data;
        private TcpClient mineClient;
        private NetworkStream stream;
        private Func<string, bool> logWriter;
        private Func<Client, bool> disconnect;

        private CrypterAES crypter;
        //private CrypterRSA rsa;

        private volatile bool isWork;
        private bool isHandshake;

        private List<long> viewedNews;
        private News uploadNews;
        private QueueDictionary<string, long> viewedNewsGlobal;

        private User uploadNewsUser;
        private User uploadUserUser;
        private User uploadUser;

        public Client(TcpClient client, ServerData serverData, Func<string, bool> LogWriter, Func<Client, bool> Disconnect, QueueDictionary<string, long> newsGlobal)
        {
            mineClient =  client;
            stream = mineClient.GetStream();
            data = serverData;
            disconnect = Disconnect;
            logWriter = LogWriter;
            stream.ReadTimeout = (int)data.milisecondsLive;
            viewedNewsGlobal = newsGlobal;
            crypter = null;
            this.uploadNewsUser = null;
            this.uploadNews =null;
        }

        ~Client()
        {
            if (stream != null)
                stream.Close();
            if (mineClient != null)
                mineClient.Close();

            disconnect.Invoke(this);
        }

        public void Dispose()
        {
            if (stream != null)
                stream.Close();
            if (mineClient != null)
                mineClient.Close();

        }

        public void Start()
        {
            isWork = true;
            isHandshake = false;
            byte[] buffer = new byte[1024];
            byte[] Data = new byte[0] { };
            int count;
            long timer = DateTime.UtcNow.Ticks;
            viewedNews = new List<long>();
            
            try
            {
                while (isWork && (timer - DateTime.UtcNow.Ticks)<data.milisecondsLive*10_000)
                {
                    count = this.stream.Read(buffer, 0, buffer.Length);
                    if (count > 0)
                    {
                        Data = Tools.concateBytes(Data, buffer, count);
                        if (!isHandshake)
                        {
                            int index = Tools.FirstIndexOf(Data, data.webSocketHendshack);
                            if (index >= 0)
                            {
                                webSocketConnect(Encoding.UTF8.GetString(Data));
                                isHandshake = true;
                                Data = new byte[0] { };
                            }
                        }

                        if (isHandshake &&  Tools.FirstIndexOf(Data, data.Headers.EndMessagesByte) < 0)
                        {                  
                            byte[] newData = webSocketDecode(Data);
                            if (Tools.FirstIndexOf(newData, data.Headers.EndMessagesByte) >= 0)
                                Data = newData;
                        }

                        while (Tools.FirstIndexOf(Data, data.Headers.EndMessagesByte) >= 0)
                        {
                            int index = Tools.FirstIndexOf(Data, data.Headers.EndMessagesByte);

                            byte[] message = Tools.SubBytes(Data, 0, index);

                            this.onMessage(message);
                            timer = DateTime.UtcNow.Ticks;
                            Data = Tools.SubBytes(Data, index + data.Headers.EndMessagesByte.Length);
                        }
                    }
                    else
                    {
                        break;
                    }

                    if (Data.Length > (int)data.maxLenMessage)
                    {
                        Data = new byte[0] { };
                        logWriter.Invoke("long Data! delete");
                    }
                }
            }
            catch (Exception e)
            {
                /*if(e.GetType() == typeof(MySqlException))
                {
                    var O = (MySqlException)e;
                    O.
                }*/
                logWriter.Invoke("Error: " + e.Message + "\n" + e.StackTrace);
            }
            this.mineClient.Close();
            this.disconnect.Invoke(this);
        }

        public void Stop()
        {
            isWork = false;
        }

        private void onMessage(byte[] Message)
        {
            if (this.crypter != null)
            {
                Message = this.crypter.DecoderBytes(Message);
            }

            logWriter.Invoke("message: " + Encoding.UTF8.GetString(Message));
            byte[][] message = Tools.Split(Message, data.Headers.SeparatorByte);
            if (message.Length == 0) return;

            string head = Encoding.UTF8.GetString(message[0]);

            if(head == data.Headers.GetAllNews && message.Length == 5)
            { 
                string type = Encoding.UTF8.GetString(message[1]), sort = Encoding.UTF8.GetString(message[2]);
                int count = Convert.ToInt32(Encoding.UTF8.GetString(message[3])), first = Convert.ToInt32(Encoding.UTF8.GetString(message[4]));
                this.getAllNews(type, sort, count, first);    
            }

            if (head == data.Headers.GetAllMyNews && message.Length == 7)
            {
                string cooka= Encoding.UTF8.GetString(message[1]),
                       type = Encoding.UTF8.GetString(message[2]),
                       sort = Encoding.UTF8.GetString(message[3]);
                int count = Convert.ToInt32(Encoding.UTF8.GetString(message[4])),
                    first = Convert.ToInt32(Encoding.UTF8.GetString(message[5]));
                bool isOnlyMine = Convert.ToBoolean(Encoding.UTF8.GetString(message[6])); ;
                this.getAllMyNews(cooka, type, sort, count, first, isOnlyMine);
            }

            if (head == data.Headers.GetNews && message.Length == 2)
            {
                int id = Convert.ToInt32(Encoding.UTF8.GetString(message[1]));
                this.getNews(id);
            }

            if (head == data.Headers.Key && message.Length == 2)
            {
                this.crypter = new CrypterAES();
                byte[] answer = Encoding.UTF8.GetBytes(data.Headers.Key + data.Headers.Separator);
                answer = Tools.concateBytes(answer, this.crypter.getKey());
                answer = Tools.concateBytes(answer, Encoding.UTF8.GetBytes(data.Headers.Separator));
                answer = Tools.concateBytes(answer, this.crypter.getIV());

                var i = new CrypterRSA(message[1]);

                this.send(i.Coder(answer));
            }
            
            if (head == data.Headers.LogIn && message.Length == 3)
            {
                string name = Encoding.UTF8.GetString(message[1]),
                    hash = Encoding.UTF8.GetString(message[2]);

                this.logIn(name, hash);             
            }

            if (head == data.Headers.LogIn && message.Length == 4)
            {
                string code = Encoding.UTF8.GetString(message[1]);
                string cooka = Encoding.UTF8.GetString(message[3]);
                bool isLodin = Convert.ToBoolean(Encoding.UTF8.GetString(message[2]));

                if (isLodin)
                {
                    this.logIn(cooka);
                }
                else
                {
                    this.logOut(cooka);
                }
                
            }

            if (head == data.Headers.DeleteNews && message.Length == 3)
            {
                long id = Convert.ToInt64(Encoding.UTF8.GetString(message[1]));
                string cooka = Encoding.UTF8.GetString(message[2]);
                   
                this.deleteNews(cooka,  id);
            }

            if( head == data.Headers.AnyNews && message.Length == 7)
            {
                News n = new News();
                n.name = Encoding.UTF8.GetString(message[1]);
                n.description = Encoding.UTF8.GetString(message[2]);
                n.photo = Encoding.UTF8.GetString(message[3]);
                n.type = Encoding.UTF8.GetString(message[4]);
                n.text = Encoding.UTF8.GetString(message[5]);
                string cooka = Encoding.UTF8.GetString(message[6]);
                this.createNews(cooka, n);
            }

            if (head == data.Headers.AnyNews && message.Length == 8)
            {
                News n = new News();
                n.id = Convert.ToInt64(Encoding.UTF8.GetString(message[1]));
                n.name = Encoding.UTF8.GetString(message[2]);
                n.description = Encoding.UTF8.GetString(message[3]);
                n.photo = Encoding.UTF8.GetString(message[4]);
                n.type = Encoding.UTF8.GetString(message[5]);
                n.text = Encoding.UTF8.GetString(message[6]);
                string cooka = Encoding.UTF8.GetString(message[7]);

                this.createNews(cooka, n);
            }

            if (head == data.Headers.GetImage && message.Length == 4)
            {
                string name =  Encoding.UTF8.GetString(message[1]),
                        data = Encoding.UTF8.GetString(message[2]),
                        cooka =Encoding.UTF8.GetString(message[3]);

                this.loadImage(cooka, name, data);
            }

            if (head == data.Headers.Complaint && message.Length == 3)
            {
                long id = Convert.ToInt64(Encoding.UTF8.GetString(message[1]));
                string cooka = Encoding.UTF8.GetString(message[2]);

                this.getComplaint(cooka, id);
            }

            if (head == data.Headers.Complaint && message.Length == 4)
            {
                long news_id = Convert.ToInt64(Encoding.UTF8.GetString(message[1])),
                     id = Convert.ToInt64(Encoding.UTF8.GetString(message[2]));
                string cooka = Encoding.UTF8.GetString(message[3]);

                this.deleteComplaint(cooka, news_id, id);
            }

            if (head == data.Headers.Complaint && message.Length == 5)
            {
                int id_news = Convert.ToInt32(Encoding.UTF8.GetString(message[1]));
                string email = Convert.ToString(Encoding.UTF8.GetString(message[2])),
                    name = Convert.ToString(Encoding.UTF8.GetString(message[3])),
                    text = Convert.ToString(Encoding.UTF8.GetString(message[4]));

                this.createComplaint(new Complaint(0, id_news, name, email, text));
            }

            if (head == data.Headers.GetAllMyEditors && message.Length == 5)
            {
                bool isAdmin = Convert.ToBoolean(Encoding.UTF8.GetString(message[1]));
                long count = Convert.ToInt64(Encoding.UTF8.GetString(message[2]));
                long offset = Convert.ToInt64(Encoding.UTF8.GetString(message[3]));
                string cooka = Encoding.UTF8.GetString(message[4]);

                this.getAllMyEditors(cooka, isAdmin, count, offset);

            }

            if (head == data.Headers.AnyEditors && message.Length == 7)
            {
                string name = Convert.ToString(Encoding.UTF8.GetString(message[1]));
                string login= Convert.ToString(Encoding.UTF8.GetString(message[2]));
                string password = Convert.ToString(Encoding.UTF8.GetString(message[3]));
                string photo= Convert.ToString(Encoding.UTF8.GetString(message[4]));
                bool isAdmin = Convert.ToBoolean(Encoding.UTF8.GetString(message[5]));
                string cooka = Encoding.UTF8.GetString(message[6]);

                this.createEditors(cooka, -1, name, login, password, photo, isAdmin);
            }

            if (head == data.Headers.AnyEditors && message.Length == 8)
            {
                long user_id = Convert.ToInt64(Encoding.UTF8.GetString(message[1]));
                string name = Convert.ToString(Encoding.UTF8.GetString(message[2]));
                string login = Convert.ToString(Encoding.UTF8.GetString(message[3]));
                string password = Convert.ToString(Encoding.UTF8.GetString(message[4]));
                string photo = Convert.ToString(Encoding.UTF8.GetString(message[5]));
                bool isAdmin = Convert.ToBoolean(Encoding.UTF8.GetString(message[6]));
                string cooka = Encoding.UTF8.GetString(message[7]);

                this.createEditors(cooka,user_id, name, login, password, photo, isAdmin);
            }

            if (head == data.Headers.AnyEditors && message.Length == 3)
            {
                long user_id = Convert.ToInt64(Encoding.UTF8.GetString(message[1]));
                string cooka = Encoding.UTF8.GetString(message[2]);

                this.getMyEditors(cooka,user_id);
            }

            if (head == data.Headers.DeleteEditor && message.Length == 4)
            {
                long user_id = Convert.ToInt64(Encoding.UTF8.GetString(message[1]));
                bool isAll = Convert.ToBoolean(Encoding.UTF8.GetString(message[2]));
                string cooka = Encoding.UTF8.GetString(message[3]);

                this.deleteMyEditors(cooka, user_id, isAll);
            }
        }

        private void deleteMyEditors(string cooka, long user_id, bool isAll)
        {
            User user = DataBase.getUserOfCookies(cooka, ((IPEndPoint)this.mineClient.Client.RemoteEndPoint).Address.ToString());
            if (user.name != null)
            {
                if (!user.isAdmin)
                {
                    this.send(data.Headers.AnyEditors, "You is not admin!");
                    return;
                }

                User u = DataBase.getUser(user_id);
                if (u.name != null)
                {
                   
                    if (isAll)
                    {
                        List<long> buffer = DataBase.getAllChildID(user_id);
                        buffer.Add(user_id);
                        DataBase.deleteAll(buffer.ToArray());
                    }
                    else
                    {
                        DataBase.deleteUser(user_id);
                        DataBase.updateAll(user_id, user.id);
                    }
                }
                else
                    this.send(data.Headers.AnyEditors, "User not found!");

            }
            else
                this.send(data.Headers.AnyEditors, "Undefined user!");
        }

        private void createEditors(string cooka, long user_id, string name, string login, string password, string photo, bool isAdmin)
        {
            if (this.uploadUser != null)
            {
                this.send(data.Headers.AnyEditors, "Not load after user!");
                return;
            }

            User user = DataBase.getUserOfCookies(cooka, ((IPEndPoint)this.mineClient.Client.RemoteEndPoint).Address.ToString());
            if (user.name != null)
            {
                if (!user.isAdmin)
                {
                    this.send(data.Headers.AnyEditors, "You is not admin!");
                    return;
                }
                User u = new User(user_id, user.id, name, login, photo, isAdmin);
                string errorMessage = User.isGoodUser(u);
                if (errorMessage != null)
                {
                    this.send(data.Headers.AnyEditors, errorMessage);
                    return;
                }
                
                if (password.Length < 8 || password.Length > 20)
                {
                    this.send(data.Headers.AnyEditors, "Invalide password!");
                    return;
                }
                u.hash = (MD5.Create()).ComputeHash(Encoding.UTF8.GetBytes(password));

                if (!DataBase.isFreeName(name, u.id))
                {
                    this.send(data.Headers.AnyEditors, "Пользователь с таким именем уже есть. Введите другое имя!");
                    return;
                }

                if (!DataBase.isFreeLogin(login, u.id))
                {
                    this.send(data.Headers.AnyEditors, "Пользователь с таким логином уже есть. Введите другой логин!");
                    return;
                }                

                this.uploadUser = u;
                this.uploadUserUser = user;
                this.loadImage(photo);
            }
            else
                this.send(data.Headers.AnyEditors, "Undefined user!");
        }

        private void getMyEditors(string cooka, long user_id)
        {
            User user = DataBase.getUserOfCookies(cooka, ((IPEndPoint)this.mineClient.Client.RemoteEndPoint).Address.ToString());
            if (user.name != null && user.isAdmin)
            {
                long[] array  = DataBase.getAllChildID(user.id).ToArray();
                foreach(long i in array)
                    if(i == user_id)
                    {
                        User u = DataBase.getUser(i);
                        string message = data.Headers.AnyEditors;

                        message += data.Headers.Separator + u.id;
                        message += data.Headers.Separator + u.idParent;
                        message += data.Headers.Separator + u.login;
                        message += data.Headers.Separator + u.name;
                        message += data.Headers.Separator + u.photo;
                        message += data.Headers.Separator + u.isAdmin;
                        message += data.Headers.Separator + u.regData;

                        this.send(message);
                        return;
                    }
                this.send(data.Headers.AnyEditors, "Not your user!");
            }
            else
                this.send(data.Headers.AnyEditors, "Undefined user!");
        }

        private void getAllMyEditors(string cooka, bool isAdmin, long count, long offset)
        {
            User user = DataBase.getUserOfCookies(cooka, ((IPEndPoint)this.mineClient.Client.RemoteEndPoint).Address.ToString());
            if (user.name != null && user.isAdmin)
            {
                long[] array;
                if (isAdmin)
                {
                    array = DataBase.getAllChildAdmin(user.id);
                }
                else
                {
                    array = DataBase.getAllChildEditor(user.id);
                }

                string message = data.Headers.GetAllMyEditors + data.Headers.Separator + data.Headers.Good + data.Headers.Separator + (offset + count > array.Length ? array.Length  - offset: count) ;
                for (int i=(int)offset; i<array.Length && i < (offset + count); i++)
                {
                    User u= DataBase.getUser(array[i]);
                    message += data.Headers.Separator + u.id;
                    message += data.Headers.Separator + u.photo;
                    message += data.Headers.Separator + u.isAdmin;
                    message += data.Headers.Separator + u.name;
                    message += data.Headers.Separator + (DataBase.getCountMyNews(u.id));
                    message += data.Headers.Separator + (DataBase.getAllChildAdmin(u.id).Length);
                }
                
                this.send(message);
            }
            else
                this.send(data.Headers.GetAllMyEditors, "Undefined user!");
        }

        private void createComplaint(Complaint c)
        {
            if (viewedNews.IndexOf(c.news_id) != -1 || this.viewedNewsGlobal.indexInQueue(((IPEndPoint)this.mineClient.Client.RemoteEndPoint).Address.ToString(), c.news_id) != -1)
            {
                if (Tools.validateEmail(c.email))
                {
                    string ip = ((IPEndPoint)this.mineClient.Client.RemoteEndPoint).Address.ToString();
                    long period = 10_000_000 * 60 * (long)data.minPeriodComplaint,
                         last = DataBase.getLastTimeComplaint(ip).Ticks;

                    if (DateTime.Now.Ticks - last > period)
                    {
                        DataBase.createComplaint(ip, c);
                        this.send(data.Headers.Complaint + data.Headers.Separator + data.Headers.Good);
                    }
                    else
                        this.send(data.Headers.Complaint + data.Headers.Separator + "Time limit! Wait " + (new DateTime(period + last) - DateTime.Now));
                }
                else
                    this.send(data.Headers.Complaint + data.Headers.Separator + "Invalide e-mail");
            }
            else
                this.send(data.Headers.Complaint + data.Headers.Separator + "Not viewed this news!");
        }

        private void getComplaint(string cooka, long id)
        {
            User user = DataBase.getUserOfCookies(cooka, ((IPEndPoint)this.mineClient.Client.RemoteEndPoint).Address.ToString());
            if (user.name != null)
            {
                News news = DataBase.getAnyNews(id, false);
                if(news != null)
                {
                    if(news.authorId != user.id)
                        if ( DataBase.getAllChildID(user.id).IndexOf(news.authorId) < 0)
                        {
                            this.send(data.Headers.Complaint, "Not yours!!");
                            return;
                        }
                    Complaint[] c = DataBase.getComplaints(news.id);

                    string message = data.Headers.Complaint + data.Headers.Separator + 
                                    news.id + data.Headers.Separator +
                                    news.name + data.Headers.Separator + 
                                    news.description + data.Headers.Separator +
                                    news.photo + data.Headers.Separator +
                                    news.views + data.Headers.Separator +
                                    news.importance + data.Headers.Separator +
                                    c.Length + data.Headers.Separator ;
                    for(int i=0; i<c.Length; i++)
                    {
                        message += c[i].id + data.Headers.Separator + c[i].name + data.Headers.Separator + c[i].email + data.Headers.Separator + c[i].text + data.Headers.Separator;
                    }

                    this.send(message);
                }
            }
            else
                this.send(data.Headers.Complaint, "Undefined user!");
        }

        private void deleteComplaint(string cooka, long news_id, long id)
        {
            User user = DataBase.getUserOfCookies(cooka, ((IPEndPoint)this.mineClient.Client.RemoteEndPoint).Address.ToString());
            if (user.name != null)
            {
                News news = DataBase.getAnyNews(news_id, false);
                if (news != null)
                {
                    if (news.authorId != user.id)
                        if (DataBase.getAllChildID(user.id).IndexOf(news.authorId) < 0)
                        {
                            this.send(data.Headers.Complaint, "Not yours!!");
                            return;
                        }
                    DataBase.deleteComplaints(news.id, id);


                    this.send(data.Headers.Complaint, data.Headers.Good);
                }
            }
            else
                this.send(data.Headers.Complaint, "Undefined user!");
        }

        private void loadNews(News news)
        {
            this.logWriter.Invoke("loadNews: " + news.id);
            if (news.id <= 0) {
                DataBase.loadNews(news);
                this.send(data.Headers.AnyNews, data.Headers.Good);
            }
            else
            {
                DataBase.updateNews(news);
                this.send(data.Headers.AnyNews, data.Headers.Good);
            }
        }

        private void loadUser(User u)
        {
            if (u.id <= 0)
                DataBase.loadUser(u, u.hash);
            else
                DataBase.updateUser(u);

            this.send(data.Headers.AnyEditors, data.Headers.Good);
        }

        private void loadImage(string image)
        {
            string name = Storage.loadImage(image);
            logWriter.Invoke("loadImage: " + name);
            if (name != null)
            {
                if (this.uploadNews != null && this.uploadNews.photo == image)
                {
                    this.uploadNews.photo = name;
                    this.loadNews(this.uploadNews);
                    this.uploadNews = null;
                }

                if (this.uploadUser != null && this.uploadUser.photo == image)
                {
                    this.uploadUser.photo = name;
                    this.loadUser(this.uploadUser);
                    this.uploadUser = null;
                }
            }
            else
            {
                this.send(data.Headers.GetImage, image);
            }          
        }

        private void createNews(string cooka, News news)
        {
            if(this.uploadNews != null)
            {
                this.send(data.Headers.AnyEditors, "Not load after news!");
                return;
            }

            User user = DataBase.getUserOfCookies(cooka, ((IPEndPoint)this.mineClient.Client.RemoteEndPoint).Address.ToString());
            if (user.name != null)
            {
                string errorMessage = News.isGoodNews(news);
                if(errorMessage != null)
                {
                    this.send(data.Headers.AnyEditors, errorMessage);
                    return;
                }

                if(!DataBase.isFreeNewsName(news.name, news.id))
                {
                    this.send(data.Headers.AnyEditors, "Новость с таким наименованием уже есть.Введите другое имя");
                    return;
                }
                this.logWriter.Invoke("cN: " + user.id + "\t: " + cooka);
                news.authorId = user.id;
                this.uploadNews = news;
                this.uploadNewsUser = user;
                this.loadImage(news.photo);
            }
            else
                this.send(data.Headers.AnyEditors, "Undefined user!");
        }

        private void deleteNews(string cooka, long id)
        {
            User user = DataBase.getUserOfCookies(cooka, ((IPEndPoint)this.mineClient.Client.RemoteEndPoint).Address.ToString());
            if (user.name != null)
            {
                News news = DataBase.getAnyNews(id, false);
                User creator = DataBase.getUser(news.authorId);

                if(creator.id == user.id)
                {
                    DataBase.deleteNews(id);
                    this.send(data.Headers.DeleteEditor, data.Headers.Good);
                }
                else
                {
                    List<long> b = DataBase.getAllChildID(user.id);
                    if(b.IndexOf(creator.id) >= 0)
                    {
                        DataBase.deleteNews(id);
                        this.send(data.Headers.DeleteEditor, data.Headers.Good);
                    }
                    else
                        this.send(data.Headers.DeleteEditor, "User can't delete this news!");
                }

            }
            else
                this.send(data.Headers.DeleteEditor, "Undefined user!");
        }

        private string createCookies(User u, string ip)
        {
            byte[] ip_hash    = new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(ip)),
                   time_hash  = new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(DateTime.Now.ToString())),
                   login_hash = new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(u.login));

            byte[] all_hash = Tools.concateBytes(ip_hash, time_hash);
            all_hash = new MD5CryptoServiceProvider().ComputeHash(Tools.concateBytes(all_hash, login_hash));

            return Tools.byteArrayToHexString(all_hash);
        }

        private void logOut(string cooka)
        {
            string ip = ((IPEndPoint)this.mineClient.Client.RemoteEndPoint).Address.ToString();
            User user = DataBase.getUserOfCookies(cooka, ip);
            if (user.name != null)
            {
                DataBase.deleteCookies(cooka, ip);
                this.send(data.Headers.LogIn, data.Headers.Good);
            }
            else
            {
                this.send(data.Headers.LogIn, "Fail user!");
            }
        }

        private void logIn(string cooka)
        {
            string ip = ((IPEndPoint)this.mineClient.Client.RemoteEndPoint).Address.ToString();
            User user = DataBase.getUserOfCookies(cooka, ip);
            if (user.name != null)
            {            
                string newCooka = createCookies(user, ip);
                DataBase.addCookies(newCooka, user.id, ip, 24);
                this.send(data.Headers.LogIn, user.name, user.photo, Convert.ToString(user.isAdmin), newCooka);         
            }
            else
            {
                this.send(data.Headers.LogIn, "Fail user!");
            }
        }

        private void logIn(string name, string password)
        {
            int lenName = name.Length, lenPassword = password.Length;
            if (lenName > 7 && lenName < 21)
            {
                if (lenPassword > 7 && lenPassword < 21)
                {
                    User u = DataBase.getUser(name, (MD5.Create()).ComputeHash(Encoding.UTF8.GetBytes(password)));
                    if (u != null)
                    {
                        string ip = ((IPEndPoint)this.mineClient.Client.RemoteEndPoint).Address.ToString();
                        string cooka = createCookies(u, ip);
                        DataBase.addCookies(cooka, u.id, ip, 24);
                        
                        this.send(data.Headers.LogIn, u.name, u.photo, Convert.ToString(u.isAdmin), cooka);
                    }
                    else
                    {
                        this.send(data.Headers.LogIn, "Логин или пароль введены неверно! Попробуйте ещё раз!");
                    }
                }
            }                
        }

        private void getNews(int id)
        {
            bool isPlus = true;

            if (viewedNews.IndexOf(id) >= 0)
                isPlus = false;
            else
            {
                if (this.viewedNewsGlobal.indexInQueue(((IPEndPoint)this.mineClient.Client.RemoteEndPoint).Address.ToString(), id) != -1)
                {
                    isPlus = false; 
                }
            }

            News news = DataBase.getAnyNews(id, isPlus);

            if (news.text != null)
            {
                if (isPlus)
                {
                    if (viewedNews.IndexOf(id) ==  -1)
                       viewedNews.Add(id);
                    if(this.viewedNewsGlobal.indexInQueue(((IPEndPoint)this.mineClient.Client.RemoteEndPoint).Address.ToString(), id) == -1)
                        this.viewedNewsGlobal.addQueue(((IPEndPoint)this.mineClient.Client.RemoteEndPoint).Address.ToString(), id);
                } 
                User creator = DataBase.getUser(news.authorId);
                this.send(data.Headers.GetNews, data.Headers.Good, Convert.ToString(news.id), news.name, news.photo, news.text, news.type, news.description, 
                    news.importance, Convert.ToString(news.views), Convert.ToString(news.regData), creator.name, creator.photo);
            }
            else
            {
                this.send(data.Headers.GetNews,"NOT FOUND");
            }           
        }

        private void getAllMyNews(string cooka, string type, string sort, int count, int offset, bool isOnlyMine)
        {
            User user = DataBase.getUserOfCookies(cooka, ((IPEndPoint)this.mineClient.Client.RemoteEndPoint).Address.ToString());
            if(user.name != null)
            {
                News[] news;
                if (user.isAdmin && isOnlyMine)
                {
                    long[] allChildID = DataBase.getAllChildID(user.id).ToArray();
                    long[] allID = new long[allChildID.Length + 1];
                    allID[0] = user.id;
                    Array.Copy(allChildID, 0, allID, 1, allChildID.Length);
                    news = DataBase.getAllNewsOfId(allID, type, sort, count, offset);
                }
                else
                    news = DataBase.getNews(user.id, type, sort, count, offset);
                string answer = data.Headers.GetAllMyNews + data.Headers.Separator + news.Length + data.Headers.Separator;

                for (int i = 0; i < news.Length; i++)
                {
                    answer += news[i].id + data.Headers.Separator;
                    answer += news[i].name + data.Headers.Separator;
                    answer += news[i].description + data.Headers.Separator;
                    answer += news[i].photo + data.Headers.Separator;
                    answer += news[i].views + data.Headers.Separator;
                    answer += news[i].importance + data.Headers.Separator;

                }
                this.send(Encoding.UTF8.GetBytes(answer));

            }
            else
                this.send(data.Headers.GetAllMyNews, "Undefined user!");


        }

        private void getAllNews(string type, string sort, int count, int offset)
        {
            News[] news = DataBase.getNews(type, sort, count, offset);

            string answer = data.Headers.GetAllNews + data.Headers.Separator + news.Length + data.Headers.Separator;

            for (int i = 0; i < news.Length; i++)
            {
                answer += news[i].id + data.Headers.Separator;
                answer += news[i].name + data.Headers.Separator;
                answer += news[i].description + data.Headers.Separator;
                answer += news[i].photo + data.Headers.Separator;
                answer += news[i].views + data.Headers.Separator;
                answer += news[i].importance + data.Headers.Separator;

            }
            this.send(Encoding.UTF8.GetBytes(answer));
        }

        private void loadImage(string cooka, string name, string data)
        {
            User user = DataBase.getUserOfCookies(cooka, ((IPEndPoint)this.mineClient.Client.RemoteEndPoint).Address.ToString());
            if (this.uploadNewsUser != null && user.id == this.uploadNewsUser.id && this.uploadNews != null)
            {
                if(this.uploadNews.photo == name)
                {
                    string newName = Storage.saveImage(name, data);
                    if(newName != "")
                    {
                        this.uploadNews.photo = newName;
                        this.loadNews(this.uploadNews);
                        this.uploadNews = null;
                    }
                    else
                        this.send(this.data.Headers.GetImage, "Error save in storage!");
                }
                else
                {
                    this.send(this.data.Headers.GetImage, "Not that!");
                }
            }else if (this.uploadUserUser != null && user.id == this.uploadUserUser.id && this.uploadUser != null)
            {
                if (this.uploadUser.photo == name)
                {
                    string newName = Storage.saveImage(name, data);
                    if (newName != "")
                    {
                        this.uploadUser.photo = newName;
                        this.loadUser(this.uploadUser);
                        this.uploadUser = null;
                    }
                    else
                        this.send(this.data.Headers.GetImage, "Error save in storage!");
                }
                else
                {
                    this.send(this.data.Headers.GetImage, "Not that!");
                }
            }
            else
            {
                this.send(this.data.Headers.GetImage, "No need!" + (this.uploadNewsUser != null? "1":"0") +  (user.id ) + this.uploadNewsUser.id + (this.uploadNews != null? "1":"0"));
            }
        }

        private void send(params string[] message)
        {
            string Message = "";
            foreach (string s in message)
            {
                Message += s + data.Headers.Separator;
            }
            Message = Message.Substring(0, Message.Length - data.Headers.Separator.Length);
            this.send(Encoding.UTF8.GetBytes(Message));
        }

        private void send(string Message)
        {
            this.send(Encoding.UTF8.GetBytes(Message));
        }

        private void send(byte[] Message)
        {
            try
            {
                if (Message == null || Message.Length == 0)
                    return;

                if (this.crypter != null)
                {
                    Message = this.crypter.CoderBytes(Message);
                }

                Message = Tools.concateBytes(Message, data.Headers.EndMessagesByte);
                
                if (this.isHandshake)
                {
                    Message = this.webSocketCode(Message);
                }

                this.stream.Write(Message, 0, Message.Length);   
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(System.Threading.ThreadAbortException)) throw e;
                //this.OnError(e);
            }
        }

        private void webSocketConnect(string request)
        {
            string answer = "HTTP/1.1 101 Switching Protocols" + "\r\n"
                + "Upgrade: websocket" + "\r\n"
                + "Connection: Upgrade" + "\r\n"
                + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                    SHA1.Create().ComputeHash(
                        Encoding.UTF8.GetBytes(
                            new Regex("Sec-WebSocket-Key: (.*)").Match(request).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                        )
                    )
                ) + "\r\n" + "\r\n";
            byte[] buff = Encoding.UTF8.GetBytes(answer);
            this.stream.Write(buff, 0, buff.Length );
        }

        private byte[] webSocketDecode(byte[] data)
        {
            if (data.Length < 7)
                return new byte[0] { };

            byte[] mask = new byte[4];
            long len;
            int ofset;

            len = (long)(data[1] & 127);
            if (len <= 125)
            {
                ofset = 2;
            }else if (len == 126)
            {
                ofset = 4;
                len = (long)(data[2]<<8);
                len |= data[3];
            }
            else
            {
                ofset = 10;
                len = data[9];
                len |= (long)(data[8] << 8);
                len |= (long)(data[7] << 16);
                len |= (long)(data[6] << 24);
                len |= (long)(data[5] << 32);
                len |= (long)(data[4] << 40);
                len |= (long)(data[3] << 48);
                len |= (long)(data[2] << 56);
            }

            if (data.Length < len + ofset + 4)
                return new byte[0] { };

            for (int i = 0; i < 4; i++)
                mask[i] = data[ofset + i];


            byte[] decode = Tools.SubBytes(data, ofset + 4, len);
            if ( (data[1] & 128) > 0)    //unmask
            {
                for (int i = 0; i < len; i++)
                    decode[i] ^= mask[i % 4];
            }

            if (data.Length > decode.Length + ofset + 4 + 7)
            {
                byte[] otherFrame = this.webSocketDecode(Tools.SubBytes(data, ofset + 4 + len));
                return Tools.ConcatByte(decode, otherFrame);
            }
            else
                return decode;
        }

        private byte[] webSocketCode(byte[] message, bool isMask = false) // ERRror code lenght! see |^|
        {
            long lenMessage = message.Length, offsetMask;
            byte[] full, mask = new byte[4];

            var rnd = new Random();
            mask[0] = (byte)rnd.Next();
            mask[1] = (byte)rnd.Next();
            mask[2] = (byte)rnd.Next();
            mask[3] = (byte)rnd.Next();

            if (lenMessage < 126)
            {
                full = new byte[lenMessage + 6];
                full[1] = (byte) (128 | ((byte)lenMessage));
                offsetMask = 2;
                

            }
            else if( lenMessage < 65535)
            {
                full = new byte[lenMessage + 8];
                full[1] = (byte)(128 | 126);
                ushort len = (ushort)lenMessage;
                full[3] = (byte) lenMessage;
                full[2] = (byte)(lenMessage>>8);
                offsetMask = 4;
            }
            else
            {
                full = new byte[lenMessage + 14];
                full[1] = (byte)(128 | 127);
                full[9] = (byte)lenMessage;
                full[8] = (byte)(lenMessage >> 8);
                full[7] = (byte)(lenMessage >> 16);
                full[6] = (byte)(lenMessage >> 24);
                full[5] = (byte)(lenMessage >> 32);
                full[4] = (byte)(lenMessage >> 40);
                full[3] = (byte)(lenMessage >> 48);
                full[2] = (byte)(lenMessage >> 56);
                offsetMask = 10;
            }

            if (!isMask)
                full[1] &= 127;

            full[0] = 129;
            if (isMask)
            {
                for (int i = 0; i < 4; i++)
                    full[offsetMask + i] = mask[i];

                for (int i = 0; i < lenMessage; i++)
                    full[offsetMask + 4 + i] = (byte)(message[i] ^ mask[i % 4]);
            }
            else
            {
                for (int i = 0; i < lenMessage; i++)
                    full[offsetMask + i] = message[i];
            }
            return full;
        }

        public bool isGood()
        {
            return isWork && this.stream != null;
        }
    }
}
