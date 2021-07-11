using System;
using MySql.Data.MySqlClient;
using System.Security.Permissions;
using System.Reflection.Metadata;
using System.Text;
using Org.BouncyCastle.Crypto.Digests;
using System.Collections.Generic;

namespace Server
{
    public static class DataBase
    {
        static private string dataBaseConnect;
        static private Func<string, bool> inLogWriter;
        
        static public bool Initialize(string nameServer, string nameDataBase, string Login, string Password, Func<string,bool> logWriter )
        {
            DataBase.inLogWriter = logWriter;
            DataBase.dataBaseConnect =   "server=" + nameServer +  
                                        ";uid=" + Login + 
                                        ";pwd=" + Password +
                                        ";database=" + nameDataBase;
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            try
            {
                connect.Open();
            }
            catch (Exception e){
                DataBase.inLogWriter.Invoke(e.Message + e.StackTrace);
                return false;
            }
            return true;
        }

        public static User getUser(string login, byte[] hash)
        {
            string query = "Select * From users where login = '" + Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(login)) +
                "' AND HASH_PASSWORD = '" + Tools.byteArrayToHexString(hash) + "';";

            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);
            User user = null;

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        user = new User();
                        user.id = Convert.ToInt64(reader.GetValue(0));
                        user.idParent = Convert.ToInt64(reader.GetValue(1));

                        user.name = Encoding.UTF8.GetString(Tools.hexStringToByteArray(Convert.ToString(reader.GetValue(2))));
                        user.login = Encoding.UTF8.GetString(Tools.hexStringToByteArray(Convert.ToString(reader.GetValue(3))));
                        user.photo = Encoding.UTF8.GetString(Tools.hexStringToByteArray(Convert.ToString(reader.GetValue(4))));

                        user.regData = Convert.ToDateTime(reader.GetValue(5));

                        user.isAdmin = Convert.ToBoolean(reader.GetValue(6));
                    }
                }
                reader.Close();
            }
            connect.Close();
            return user;
        }

        public static User getUser(long id)
        {
            string query = "Select * From users where id = " + id + ";";
            User user = new User();

            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        user.id = Convert.ToInt64(reader.GetValue(0));
                        user.idParent = Convert.ToInt64(reader.GetValue(1));

                        user.name = Encoding.UTF8.GetString(Tools.hexStringToByteArray(Convert.ToString(reader.GetValue(2))));
                        user.login = Encoding.UTF8.GetString(Tools.hexStringToByteArray(Convert.ToString(reader.GetValue(3))));
                        user.photo = Encoding.UTF8.GetString(Tools.hexStringToByteArray(Convert.ToString(reader.GetValue(4))));

                        user.regData = Convert.ToDateTime(reader.GetValue(5));

                        user.isAdmin = Convert.ToBoolean(reader.GetValue(6));
                    }
                }
                reader.Close();
            }
            connect.Close();
            return user;
        }

        public static bool deleteUser(long id)
        {
            string query = "delete From users where id = " + id + ";";
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);
            bool answer = command.ExecuteNonQuery() > 0;
            connect.Close();
            return answer;
        }

        public static User getUserOfCookies(string cooka, string ip)
        {
            string query = "Select * From cookies where cooka = '" + Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(cooka)) + "';",
                   query2= "Delete from cookies where id = 0";

            long id, id_user = 0;
            string ip_cooka, time_out;

            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                    while (reader.Read())
                    {
                        id = Convert.ToInt64(reader.GetValue(0));
                        ip_cooka = Encoding.UTF8.GetString(Tools.hexStringToByteArray(Convert.ToString(reader.GetValue(1))));
                        id_user = Convert.ToInt64(reader.GetValue(3));
                        time_out = Convert.ToString(reader.GetValue(4));

                        if (ip == ip_cooka && DateTime.Now < Convert.ToDateTime(time_out))
                            break;
                        else
                        {
                            query2 += " or id = " + id;
                            id_user = -1;
                        }
                            
                    }
                reader.Close();
            }

            new MySqlCommand(query2 + ";", connect).ExecuteNonQuery();
            connect.Close();
            return getUser(id_user);
        }

        public static News[] getNews(string type, string sort, int count, int first)
        {
            string query = "Select id, views, author_id, name, description, importance, photo  From news";

            query = DataBase.getNewsQuery(query, type, sort, count, first);

            return DataBase.getNewsOfQuery(query, count);
        }

        public static News[] getNews(long author_id, string type, string sort, int count, int first)
        {
            string query = "Select id, views, author_id, name, description, importance, photo  From news where author_id=" + author_id;

            query = DataBase.getNewsQuery(query, type, sort, count, first);
       
            return DataBase.getNewsOfQuery(query, count);
        }

        public static News[] getAllNewsOfId(long[] allID, string type, string sort, int count, int offset)
        {
            string query = "Select id, views, author_id, name, description, importance, photo  From news where author_id=0";
            foreach(long id in allID)
                query += " or author_id=" + id;

            query = DataBase.getNewsQuery(query, type, sort, count, offset);
            return DataBase.getNewsOfQuery(query, count);
        }

        private static string getNewsQuery(string startRequest, string type, string sort, int count, int offset)
        {
            string whereORand;
            if (startRequest.IndexOf("where") > 0)
            {
                whereORand  = " and ";
            }
            else
            {
                whereORand = " where ";
            }
            switch (type)
            {
                case "POLITICS":
                    startRequest += whereORand + " type = 'POLITICS'";
                    break;
                case "ECONOMY":
                    startRequest += whereORand +" type = 'ECONOMY'";
                    break;
                case "SPORT":
                    startRequest += whereORand +" type = 'SPORT'";
                    break;
                case "SOCIETY":
                    startRequest += whereORand +" type = 'SOCIETY'";
                    break;
            }

            if (sort == "TIME")
                startRequest += " ORDER BY reg_data DESC";
            else if (sort == "POPULARITY")
                startRequest += " ORDER BY views DESC";
            else
                startRequest += " ORDER BY importance DESC";

            startRequest += " LIMIT " + count + " OFFSET " + offset + ";";
            return startRequest;
        }

        private static News[] getNewsOfQuery(string query, int count)
        {
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);
            News[] allNews = new News[count];
            News buffer;
            int index = 0;
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        buffer = new News();

                        buffer.id = Convert.ToInt32(reader.GetValue(0));
                        buffer.views = Convert.ToInt32(reader.GetValue(1));
                        buffer.authorId = Convert.ToInt32(reader.GetValue(2));
                        buffer.name = Encoding.UTF8.GetString(Tools.hexStringToByteArray(Convert.ToString(reader.GetValue(3))));
                        buffer.description = Encoding.UTF8.GetString(Tools.hexStringToByteArray(Convert.ToString(reader.GetValue(4))));
                        buffer.importance = Convert.ToString(reader.GetValue(5));
                        buffer.photo = Encoding.UTF8.GetString(Tools.hexStringToByteArray(Convert.ToString(reader.GetValue(6))));

                        allNews[index++] = buffer;
                    }
                }
                Array.Resize(ref allNews, index);
                reader.Close();
            }
            connect.Close();
            return allNews;
        }

        public static long getCountMyNews(long user_id)
        {
            string query = "select count(*) from news where author_id=" + user_id + ";";
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);
            long answer = (long)(command.ExecuteScalar());
            connect.Close();
            return answer;
        } 

        private static bool isThereNews(long id)
        {
            string query = "select count(id) from news where id=" + id + ";";
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);
            bool answer = (long)(command.ExecuteScalar()) != 0;
            return answer;
        } 

        public static News getAnyNews(long id, bool isPlus)
        {
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            if (isPlus)
            {
                string q = "UPDATE news SET views = views + 1 WHERE  id = " + id + ";";
                MySqlCommand c = new MySqlCommand(q, connect);
                c.ExecuteNonQuery();
            }
                
            string query = "Select id, views, author_id, name, text, importance, photo, reg_data, type, description  From news WHERE id=" + id + ";";

            MySqlCommand command = new MySqlCommand(query, connect);

            News buffer = null;
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        buffer = new News();

                        buffer.id = Convert.ToInt32(reader.GetValue(0));
                        buffer.views = Convert.ToInt32(reader.GetValue(1));
                        buffer.authorId = Convert.ToInt32(reader.GetValue(2));
                        buffer.name = Encoding.UTF8.GetString(Tools.hexStringToByteArray(Convert.ToString(reader.GetValue(3))));
                        buffer.text = Encoding.UTF8.GetString(Tools.hexStringToByteArray(Convert.ToString(reader.GetValue(4))));
                        buffer.importance = Convert.ToString(reader.GetValue(5));
                        buffer.photo = Encoding.UTF8.GetString(Tools.hexStringToByteArray(Convert.ToString(reader.GetValue(6))));
                        buffer.regData = Convert.ToDateTime(reader.GetValue(7));
                        buffer.type = Convert.ToString(reader.GetValue(8));
                        buffer.description = Encoding.UTF8.GetString(Tools.hexStringToByteArray(Convert.ToString(reader.GetValue(9))));
                    }
                }
                reader.Close();
            }
            connect.Close();
            return buffer;
        }
        public static bool updateNews(News news)
        {
            if (DataBase.isThereNews(news.id))
            {
                string query = "update news set name='";
                query += Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(news.name)) + "', description='";
                query += Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(news.description)) + "', type='";
                query += news.type + "', photo='";
                query += Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(news.photo)) + "', text='";
                query += Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(news.text)) + "';";

                MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
                connect.Open();
                MySqlCommand command = new MySqlCommand(query, connect);
                bool answer = command.ExecuteNonQuery() != 0;
                connect.Close();
                return answer;
            }
            else
            {
                return DataBase.loadNews(news);
            }
        }

        public static bool loadNews(News news)
        {
            string query = "Insert into news(name, author_id, description, type,  photo, text) values( '";
            query += Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(news.name)) + "', ";
            query += news.authorId + ", '";
            query += Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(news.description)) + "', '";
            query += news.type + "', '";
            query += Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(news.photo)) + "', '";
            query += Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(news.text)) + "');";
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);
            bool answer = command.ExecuteNonQuery() != 0;
            connect.Close();
            return answer;
        }

        public static bool deleteNews(long id)
        {
            string query = "Delete From news WHERE id=" + id + ";";
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query,connect);
            bool answer = command.ExecuteNonQuery() > 0;
            connect.Close();
            return answer;
        }

        public static bool deleteCookies(string cooka, string ip)
        {
            string query = "delete from /*7*/ cookies where cooka = '" + 
                Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(cooka)) + "' and ip = '" + 
                Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(ip)) + "';";
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);
            bool answer = command.ExecuteNonQuery() > 0;
            connect.Close();
            return answer;
        }

        public static bool addCookies(string cooka, long idUser, string ip, int hourLive)
        {
            //delete old cookies or any cookies of this IP
            string query = "Select * From cookies where cooka = '" + Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(cooka)) + "' or ip = '" + Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(ip)) + "';";
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);
            query = "delete from cookies where id=0";

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                    while (reader.Read()) 
                        query += " or id=" + Convert.ToInt32(reader.GetValue(0));
                reader.Close();
            }
            new MySqlCommand(query + ";", connect).ExecuteNonQuery();
            
            //Insert new cookie
            query = "Insert into cookies(ip, cooka, id_user,  time_out) values('" +
                Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(ip)) + "', '" +
                Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(cooka)) + "', " +
                idUser + ", '" +
                DateTime.Now.AddHours(hourLive).ToString("yyyyMMddHHmmss") + "');";
            bool answer = (new MySqlCommand(query, connect)).ExecuteNonQuery() > 0;
            connect.Close();
            return answer;
        }

        public static void getIDforFirstChild(long user_id, out List<long> idAdmin, out List<long> idEditor)
        {
            idAdmin = new List<long>();
            string query = "Select id From users where parent_id = " + user_id + " and IS_ADMIN = true;";
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                    while (reader.Read())
                        idAdmin.Add( Convert.ToInt64(reader.GetValue(0)));
                reader.Close();
            }

            idEditor = new List<long>();
            query = "Select id From users where parent_id = " + user_id + " and IS_ADMIN = false;";
            MySqlCommand command2 = new MySqlCommand(query, connect);

            using (MySqlDataReader reader = command2.ExecuteReader())
            {
                if (reader.HasRows)
                    while (reader.Read())
                        idEditor.Add(Convert.ToInt64(reader.GetValue(0)));
                reader.Close();
            }
            connect.Close();
        }

        public static long[] getAllChildAdmin( long user_id)
        {
            List<long> idAdmin, idEditor;
            getIDforFirstChild(user_id, out idAdmin, out idEditor);

            if (idAdmin.Count != 0)
            {
                List<long> newId = new List<long>();
                foreach (long child_id in idAdmin)
                    newId.AddRange(getAllChildAdmin(child_id));
                idAdmin.AddRange(newId);
            }
            return idAdmin.ToArray();
        }

        public static long[] getAllChildEditor(long user_id)
        {
            List<long> idAdmin, idEditor;

            getIDforFirstChild(user_id, out idAdmin, out idEditor);

            if (idAdmin.Count != 0)
            {
                foreach (long child_id in idAdmin)
                    idEditor.AddRange(getAllChildEditor(child_id));
            }

            return idEditor.ToArray();
        }

        public static List<long> getAllChildID(long user_id)
        {
            List<long> idAdmin, idEditor;

            getIDforFirstChild(user_id, out idAdmin, out idEditor);

            if (idAdmin.Count != 0)
            {
                foreach (long child_id in idAdmin)
                    idEditor.AddRange(getAllChildID(child_id));
            }

            idEditor.AddRange(idAdmin);
            return idEditor;
        }

        public static DateTime getLastTimeComplaint(string ip)
        {
            string query = "Select MAX(reg_data) From complaints WHERE ip='" + ip + "';";
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);

            object time = command.ExecuteScalar();
            connect.Close();
            if (time.GetType() != typeof(System.DBNull))
                return Convert.ToDateTime(time);
            else
                return new DateTime(0);
        }

        public static bool createComplaint(string ip, Complaint c)
        {
            string query = "INSERT INTO complaints (ip, id_news, name, email, text) VALUE( '" + 
                    ip + "'," +
                    c.news_id + ",'" +
                    Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(c.name)) + "','" +
                    Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(c.email)) + "','" +
                    Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(c.text)) +"');";
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);
            bool answer = command.ExecuteNonQuery() > 0;
            connect.Close();
            return answer;
        }

        public static Complaint[] getComplaints( long news_id)
        {
            string query = "select id, id_news, name, email, text from complaints where id_news = " + news_id + ";";
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);
            List<Complaint>  comp = new List<Complaint>();

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var buffer = new Complaint(Convert.ToInt64(reader.GetValue(0)), 
                                                    Convert.ToInt64(reader.GetValue(1)),
                                                    Encoding.UTF8.GetString(Tools.hexStringToByteArray(Convert.ToString(reader.GetValue(2)))),
                                                    Encoding.UTF8.GetString(Tools.hexStringToByteArray(Convert.ToString(reader.GetValue(3)))),
                                                    Encoding.UTF8.GetString(Tools.hexStringToByteArray(Convert.ToString(reader.GetValue(4)))));
                        comp.Add(buffer);
                    }
                }
                reader.Close();
            }
            connect.Close();
            return comp.ToArray();
        }

        public static bool deleteComplaints(long news_id, long id)
        {
            string query = "delete from complaints where id_news = " + news_id + " and id=" + id + ";";
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);
            bool answer = command.ExecuteNonQuery() > 0;
            connect.Close();
            return answer;
        }

        public static bool isFreeName(string name, long user_id)
        {
            string query = "select /*1*/ count(*) from users where name LIKE '" + Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(name)) + "' and id != " + user_id + ";";
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);
            bool answer = (long)(command.ExecuteScalar()) == 0;
            connect.Close();
            return answer;
        }

        public static bool isFreeLogin(string login, long user_id)
        {
            string query = "select /*4*/ count(*) from users where login LIKE '" + Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(login)) + "'and id != " + user_id + ";";
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);
            bool answer = (long)(command.ExecuteScalar()) == 0;
            connect.Close();
            return answer;
        }

        public static bool isFreeNewsName(string name, long news_id)
        {
            string query = "select /*15*/ count(*) from news where name LIKE '" + Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(name)) + "' and id != " + news_id + ";";
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);
            bool answer = (long)(command.ExecuteScalar()) == 0;
            connect.Close();
            return answer;
        }

        public static bool loadUser(User u, byte[] hash)
        {
            string query = "INSERT /*2*/INTO users (parent_id, NAME, LOGIN, PHOTO, IS_ADMIN, HASH_PASSWORD) VALUE( " +
                    u.idParent + ", '" +
                    Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(u.name)) + "','" +
                    Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(u.login)) + "','" +
                    Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(u.photo)) + "', " +
                    (u.isAdmin ? 1 : 0 ) + ", '" +
                    Tools.byteArrayToHexString(hash)    + "');";
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);
            bool answer = command.ExecuteNonQuery() > 0;
            connect.Close();
            return answer;
        }

        public static bool updateUser(User u)
        {
            string query = "UPDATE /*3*/ users SET parent_id = " +
                u.idParent + ", NAME = '" +
                Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(u.name)) + "', LOGIN = '" +
                Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(u.login)) + "', PHOTO = '" +
                Tools.byteArrayToHexString(Encoding.UTF8.GetBytes(u.photo)) + "', IS_ADMIN = " +
                (u.isAdmin ? 1 : 0);
            if (u.hash != null && u.hash.Length > 0)
                query += ", HASH_PASSWORD ='" + Tools.byteArrayToHexString(u.hash) + "' where id=" + u.id + ";";
            else
                query += " where id=" + u.id + ";";
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);
            bool answer = command.ExecuteNonQuery() > 0;
            connect.Close();
            return answer;
        }

        public static bool updateAll(long oldID, long newID)
        {
            string query = "UPDATE /*5.1*/ users SET parent_id = " + newID + " where parent_id=" + oldID + ";";
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);
            bool usersUpdate = command.ExecuteNonQuery() > 0;

            query = "UPDATE /*5.2*/ news SET author_id = " + newID + " where author_id=" + oldID + ";";
            MySqlCommand command2 = new MySqlCommand(query, connect);
            bool answer = command2.ExecuteNonQuery() > 0 || usersUpdate;
            connect.Close();
            return answer;
        }

        public static bool deleteAll(long[] deleteID)
        {
            string whreIN = "";
            for (int i = 0; i < deleteID.Length; i++)
                whreIN += deleteID[i] + ", ";
            whreIN = whreIN.Substring(0, whreIN.LastIndexOf(','));

            string query = "DELETE FROM/*6.1*/ users where id in (" + whreIN + ");";
            DataBase.inLogWriter.Invoke("delete: " +query);
            MySqlConnection connect = new MySqlConnection(DataBase.dataBaseConnect);
            connect.Open();
            MySqlCommand command = new MySqlCommand(query, connect);
            bool usersDelete = command.ExecuteNonQuery() > 0;

            query = "DELETE FROM/*6.2*/ news where author_id in (" + whreIN + ");";
            MySqlCommand command2 = new MySqlCommand(query, connect);
            bool answer = command2.ExecuteNonQuery() > 0 || usersDelete;
            connect.Close();
            return answer;
        }
    }
}
