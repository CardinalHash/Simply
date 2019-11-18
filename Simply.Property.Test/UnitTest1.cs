using NUnit.Framework;
using System;
using System.Globalization;

namespace Simply.Property.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {

            CultureInfo enUS = new CultureInfo("en-US");
            string str = "20191025";
            var dt = DateTime.TryParseExact(str, "yyyyMMdd", enUS, DateTimeStyles.None, out DateTime dateTime);

            var i = typeof(char).IsPrimitive;

            Assert.Pass();
        }
    }
}