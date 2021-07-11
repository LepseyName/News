using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class News
    {
        public string name;
        public string description;
        public string text;
        public string photo;
        public string type;
        public string importance;

        public long id;
        public int views;
        public long authorId;

        public DateTime regData;


        public News() { }

        public News(string name, string description, string text, string photo, string type)
        {
            this.name = name;
            this.description = description;
            this.text = text;
            this.photo = photo;
            this.type = type;
        }

        public static string isGoodNews(News news)
        {
            if (news.name.Length < 10 || news.name.Length > 20)
                return "Название  должно быть от 10 до 20 символов!";

            if (news.description.Length < 50 || news.description.Length > 255)
                return "Описание должно быть от 50 до 255 символов!";
            switch (news.type)
            {
                case "POLITICS": break;
                case "ECONOMY": break;
                case "SPORT": break;
                case "SOCIETY": break;
                default:
                    return "Invalide type!";       
            }
            if (news.text.Length < 50 || news.text.Length > 40960)
                return "Текст должен быть от 50 до 40960!";    
            if (news.photo.Length < 4 || news.photo.Length > 200)
                return "Invalide photo!";
            return null;                
        }

    }
}
