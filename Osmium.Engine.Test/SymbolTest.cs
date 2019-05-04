using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Osmium.Engine.Test
{
    public class SymbolTest
    {
        private OsmiumEngine _engine;

        [SetUp]
        public void SetUp()
        {
            _engine = new OsmiumEngine();
        }

        [Test]
        public void TestRefEquals()
        {
            var a1 = _engine.GetSymbol("a");
            var a2 = _engine.GetSymbol("a");

            Assert.AreSame(a1, a2);
        }
    }
}
