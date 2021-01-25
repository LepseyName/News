using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class User
    {
        public long id;
        public long idParent;

        public string name;
        public string login;
        public string photo;

        public DateTime regData;
        public bool isAdmin;

        public byte[] hash;

        public User(long id, long idParent, string name, string login, string photo, bool isAdmin)
        {
            this.id = id;
            this.idParent = idParent;
            this.name = name;
            this.login = login;
            this.photo = photo;
            this.isAdmin = isAdmin;
        }

        public User() {}

        public  static bool isGoodChar(string str)
        {
            string goodChar = "qwertyuiopasdfghjklzxcvbnm1234567890_";
            str = str.ToLower();
            for (int i = 0; i < str.Length; i++)
                if (goodChar.IndexOf(str[i]) < 0)
                    return false;
            return true;
        }

        public static string isGoodUser(User user)
        {
            if (user.name.Length < 10 || user.name.Length > 20)
                return "Invalide name!";       

            if (user.login.Length < 8 || user.login.Length > 20)
                return "Invalide login!";

            if (user.photo.Length < 4 || user.photo.Length > 200)
                return "Invalide photo!";

            if (!User.isGoodChar(user.login))
                return "Логин должен содержать латинские символы, цифры и знак подчеркивания!";

            return null;
        }
    }
}
