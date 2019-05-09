using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Osmium.Engine.Test
{
    public class ParserTests
    {
        private OsmiumEngine _engine;
        

        [SetUp]
        public void SetUp()
        {
            _engine = new OsmiumEngine();
        }

        [Test]
        public void TestSymbolParse()
        {
            void Helper(string goal, string input)
            {
                Assert.AreEqual(_engine.Sym(goal), _engine.Evaluate(input));
            }

            Helper("Global`a", "a");
            Helper("Global`a", "Global`a");
            Helper("Global`x`y", "`x`y");
        }

        [Test]
        public void TestIntParse()
        {
            Assert.AreEqual(_engine.Num(1), _engine.Parse("1"));
        }

        [Test]
        public void TestDoubleParse()
        {
            Assert.AreEqual(_engine.Num(1.0), _engine.Parse("1.0"));
        }

        [Test]
        public void TestStringParse()
        {
            Assert.AreEqual(_engine.Str("abcd"), _engine.Evaluate("\"abcd\""));
        }

        [Test]
        public void TestFullForm()
        {
            var f = _engine.Sym("f");
            var x = _engine.Sym("x");
            var nul = _engine.Null;

            Assert.AreEqual(_engine.Expr(f), _engine.Evaluate("f[]"));
            Assert.AreEqual(_engine.Expr(f, x), _engine.Evaluate("f[x]"));
            Assert.AreEqual(_engine.Expr(f, x, x), _engine.Evaluate("f[x, x]"));
            Assert.AreEqual(_engine.Expr(f, x, nul, x), _engine.Evaluate("f[x, , x]"));
        }

        [Test]
        public void TestNewlineEscaping()
        {
            Assert.AreEqual(_engine.Num(1), _engine.Parse("1\n2"));
            Assert.AreEqual(_engine.Parse("f[1 * 2]"), _engine.Parse("f[1 *\n2]"));
        }

        [TestCase("Slot[1]", "#")]
        [TestCase("Slot[2]", "#2")]
        [TestCase("SlotSequence[1]", "##")]
        [TestCase("SlotSequence[2]", "##2")]

        [TestCase("Out[]", "%")]
        [TestCase("Out[2]", "%2")]

        [TestCase("Blank[ ]", "_")]
        [TestCase("Blank[h]", "_h")]
        [TestCase("BlankSequence[ ]", "__")]
        [TestCase("BlankSequence[h]", "__h")]
        [TestCase("BlankNullSequence[ ]", "___")]
        [TestCase("BlankNullSequence[h]", "___h")]
        [TestCase("Optional[Blank[]]", "_.")]
        [TestCase("Pattern[x, Blank[ ]]", "x_")]
        [TestCase("Pattern[x, Blank[h]]", "x_h")]
        [TestCase("Pattern[x, BlankSequence[ ]]", "x__")]
        [TestCase("Pattern[x, BlankSequence[h]]", "x__h")]
        [TestCase("Pattern[x, BlankNullSequence[ ]]", "x___")]
        [TestCase("Pattern[x, BlankNullSequence[h]]", "x___h")]
        [TestCase("Optional[Pattern[x, Blank[]]]", "x_.")]

        [TestCase("a ? b", "PatternTest[a, b]")]
        [TestCase("a[[b]]", "Part[a, b]")]
        [TestCase("a[[b, c]]", "Part[a, b, c]")]
        [TestCase("a++", "Increment[a]")]
        [TestCase("a--", "Decrement[a]")]
        [TestCase("++a", "PreIncrement[a]")]
        [TestCase("--a", "PreDecrement[a]")]
        public void TestEqual(string a, string b)
        {
            var expected = _engine.Parse(a);
            var actual = _engine.Parse(b);
            Assert.IsTrue(expected.Equals(actual));
        }
    }
}
