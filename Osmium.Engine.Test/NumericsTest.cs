using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Osmium.Engine.Test
{
    public class NumericsTest
    {
        private OsmiumEngine _e;

        [SetUp]
        public void SetUp()
        {
            _e = new OsmiumEngine();
        }

        [Test]
        public void TestIntegers()
        {
            var a = _e.Num(1);
            var b = _e.Num(2);

            Assert.AreEqual(_e.Num(3), a + b);
            Assert.AreEqual(_e.Num(-1), a - b);
            Assert.AreEqual(b, a * b);
        }
    }
}
