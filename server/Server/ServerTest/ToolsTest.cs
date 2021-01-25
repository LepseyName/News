using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Server;

namespace ServerTest
{
    [TestFixture]
    class ToolsTest
    {
        const string Str10 = "1234567890";
        const string Str10Bad = "1234567$$$";
        const string Str55 = "1234567890123456789012345678901234567890123456789012345";
        const string Str300 = "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
        const string Str3 = "123";

        [Test]
        public void validateEmail_GoodCharBadHost_False()
        {
            string email = "user@notReal.host";
            bool actualValue = Tools.validateEmail(email);
            Assert.IsFalse(actualValue);
        }

        [Test]
        public void validateEmail_BadCharBadHost_False()
        {
            string email = "us^er@notReal.host";
            bool actualValue = Tools.validateEmail(email);
            Assert.IsFalse(actualValue);
        }

        [Test]
        public void validateEmail_BadCharGoodHost_False()
        {
            string email = "us^er@yandex.ru";
            bool actualValue = Tools.validateEmail(email);
            Assert.IsFalse(actualValue);
        }

        [Test]
        public void validateEmail_GoodCharGoodHost_True()
        {
            string email = "user@yandex.ru";
            bool actualValue = Tools.validateEmail(email);
            Assert.IsTrue(actualValue);
        }


        [Test]
        [TestCase(Str10, Str55, Str55, Str55, "SOCIETY")]
        public void isGoodNews_GoodNameGoodDescriptionGoodTextGoodTypeGoodPhoto_Null(string name, string description, string text, string photo, string type)
        {
            string actualValue = News.isGoodNews(new News(name, description, text, photo, type));          
            Assert.IsNull(actualValue);
        }

        [Test]
        [TestCase(Str3, Str55, Str55, Str55, "SOCIETY")]
        [TestCase(Str55, Str55, Str55, Str55, "SOCIETY")]
        public void isGoodNews_BadNameGoodDescriptionGoodTextGoodTypeGoodPhoto_Message(string name, string description, string text, string photo, string type)
        {
            string actualValue = News.isGoodNews(new News(name, description, text, photo, type));
            Assert.AreEqual("Название  должно быть от 10 до 20 символов!", actualValue);
        }

        [Test]
        [TestCase(Str10, Str10, Str55, Str55, "SOCIETY")]
        [TestCase(Str10, Str300, Str55, Str55, "SOCIETY")]
        public void isGoodNews_GoodNameBadDescriptionGoodTextGoodTypeGoodPhoto_Message(string name, string description, string text, string photo, string type)
        {
            string actualValue = News.isGoodNews(new News(name, description, text, photo, type));
            Assert.AreEqual("Описание должно быть от 50 до 255 символов!", actualValue);
        }

        [Test]
        [TestCase(Str10, Str55, Str10, Str55, "SOCIETY")]
        public void isGoodNews_GoodNameGoodDescriptionBadTextGoodTypeGoodPhoto_Message(string name, string description, string text, string photo, string type)
        {
            string actualValue = News.isGoodNews(new News(name, description, text, photo, type));
            Assert.AreEqual("Текст должен быть от 50 до 40960!", actualValue);
        }

        [Test]
        [TestCase(Str10, Str55, Str55, Str55, "SOCIETY_")]
        [TestCase(Str10, Str55, Str55, Str55, "ALL")]
        public void isGoodNews_GoodNameGoodDescriptionGoodTextBadTypeGoodPhoto_Message(string name, string description, string text, string photo, string type)
        {
            string actualValue = News.isGoodNews(new News(name, description, text, photo, type));
            Assert.AreEqual("Invalide type!", actualValue);
        }

        [Test]
        [TestCase(Str10, Str55, Str55, Str3, "SOCIETY")]
        [TestCase(Str10, Str55, Str55, Str300, "SOCIETY")]
        public void isGoodNews_GoodNameGoodDescriptionGoodTextGoodTypeBadPhoto_Message(string name, string description, string text, string photo, string type)
        {
            string actualValue = News.isGoodNews(new News(name, description, text, photo, type));
            Assert.AreEqual("Invalide photo!", actualValue);
        }

        [Test]
        [TestCase(Str10, Str10, Str10)]
        public void isGoodUser_GoodNameGoodLoginGoodPhoto_Null(string name, string login, string photo)
        {
            string actualValue = User.isGoodUser(new User(0, 0, name, login, photo, false));
            Assert.IsNull(actualValue);
        }

        [Test]
        [TestCase(Str3, Str10, Str10)]
        [TestCase(Str55, Str10, Str10)]
        public void isGoodUser_BadNameGoodLoginGoodPhoto_Message(string name, string login, string photo)
        {
            string actualValue = User.isGoodUser(new User(0, 0, name, login, photo, false));
            Assert.AreEqual("Invalide name!", actualValue);
        }

        [Test]
        [TestCase(Str10, Str3, Str10)]
        [TestCase(Str10, Str55, Str10)]
        public void isGoodUser_GoodNameBadLoginGoodPhoto_Message(string name, string login, string photo)
        {
            string actualValue = User.isGoodUser(new User(0, 0, name, login, photo, false));
            Assert.AreEqual("Invalide login!", actualValue);
        }

        [Test]
        [TestCase(Str10, Str10Bad, Str10)]
        public void isGoodUser_GoodNameBadLoginCharGoodPhoto_Message(string name, string login, string photo)
        {
            string actualValue = User.isGoodUser(new User(0, 0, name, login, photo, false));
            Assert.AreEqual("Логин должен содержать латинские символы, цифры и знак подчеркивания!", actualValue);
        }

        [Test]
        [TestCase(Str10, Str10, Str3)]
        [TestCase(Str10, Str10, Str300)]
        public void isGoodUser_GoodNameGoodLoginBadPhoto_Message(string name, string login, string photo)
        {
            string actualValue = User.isGoodUser(new User(0, 0, name, login, photo, false));
            Assert.AreEqual("Invalide photo!", actualValue);
        }

        [Test]
        [TestCase(Str10)]
        [TestCase(Str300)]
        public void isGoodChar_GoodChar_True(string str)
        {
            Assert.IsTrue(User.isGoodChar(str));
        }

        [Test]
        public void isGoodChar_NullString_True()
        {
            Assert.IsTrue(User.isGoodChar(""));
        }

        [Test]
        [TestCase(Str10Bad)]
        public void isGoodChar_BadChar_False(string str)
        {
            Assert.IsFalse(User.isGoodChar(str));
        }
    }
}
