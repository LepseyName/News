using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
   
    public class Complaint
    {
        public long id;
        public long news_id;
        public string name;
        public string email;
        public string text;

        public Complaint(long id, long news_id, string name, string email, string text)
        {
            this.id = id;
            this.news_id = news_id;
            this.name = name;
            this.email = email;
            this.text = text;
        }
    }
}
