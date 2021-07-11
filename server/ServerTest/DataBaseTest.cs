using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Server;

namespace ServerTest
{
    [TestFixture]
    class DataBaseTest
    {
        const string BadCooka  = "1234567890";
        const string GoodCooka = "1234567$$$";
        const string BadIP  = "1234";
        const string GoodIP = "12345";

        const long GoodID =  157896;
        const long BadID = -12;

        [SetUp]
        public void Setup()
        {
            DataBase.Initialize("localhost", "bntunews", "server1", "q4A5dEltA", null);
            DataBase.addCookies(GoodCooka, 7, GoodIP, 1);
            /*
            News n = new News("", "", "", "", "");
            n.id = GoodID;
            DataBase.loadNews(n);
            */
        }
        /*
        [Test]
        public void deleteNews_BadNewsID_True()
        {
            bool actualValue = DataBase.deleteNews(BadID);
            Assert.IsTrue(actualValue);
        }


        [Test]
        public void deleteNews_GoodNewsID_True()
        {
            bool actualValue = DataBase.deleteNews(GoodID);
            Assert.IsTrue(actualValue);
        }
        */
        [Test]
        public void getUserOfCookies_GoodCookaGoodIP_NotNull()
        {
            User actualValue = DataBase.getUserOfCookies(GoodCooka, GoodIP);
            Assert.IsNotNull(actualValue.name);
        }

        [Test]
        public void getUserOfCookies_BadCookaGoodIP_Null()
        {
            User actualValue = DataBase.getUserOfCookies(BadCooka, GoodIP);
            Assert.IsNull( actualValue.name);
        }

        [Test]
        public void getUserOfCookies_GoodCookaBadIP_Null()
        {
            User actualValue = DataBase.getUserOfCookies(GoodCooka, BadIP);
            Assert.IsNull(actualValue.name);
        }

        [Test]
        public void getUserOfCookies_BadCookaBadIP_Null()
        {
            User actualValue = DataBase.getUserOfCookies(BadCooka, BadIP);
            Assert.IsNull(actualValue.name);
        }

    }
}
