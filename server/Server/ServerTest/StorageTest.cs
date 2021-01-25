using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Server;

namespace ServerTest
{
    [TestFixture]
    class StorageTest
    {
        const string BadUrl = "1234567890";
        const string GoodUrl = "C:\\Users\\LName\\Desktop\\CB-obs_XIAA5EVg.jpg";
        const string BadPath = "C:\\1234567890";
        const string GoodPath = "test/log.txt";

        [SetUp]
        public void Setup()
        {
            Storage.setBasePatchImage("test");
        }

        [Test]
        public void logInitialize_GoodPath_True()
        {
            bool actualValue = Storage.logInitialize(GoodPath);
            Assert.IsTrue(actualValue);
        }

        [Test]
        public void logInitialize_BadPath_False()
        {
            bool actualValue = Storage.logInitialize(BadPath);
            Assert.IsFalse(actualValue);
        }

        [Test]
        public void loadImage_BadUrl_Null()
        {
            string actualValue = Storage.loadImage(BadUrl);
            Assert.IsNull(actualValue);
        }

        [Test]
        public void loadImage_GoodUrl_NotNull()
        {
            string actualValue = Storage.loadImage(GoodUrl);
            Assert.IsNotNull(actualValue);
        }

    }
}
