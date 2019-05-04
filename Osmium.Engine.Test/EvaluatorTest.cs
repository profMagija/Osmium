using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Osmium.Engine.Values;

namespace Osmium.Engine.Test
{
    public class EvaluatorTest
    {
        private OsmiumEngine _e;

        Value ValOf(string sym) => _e.Evaluate(_e.Sym(sym));

        [SetUp]
        public void SetUp()
        {
            _e = new OsmiumEngine();
        }

        [Test]
        public void TestSetOwnValue()
        {
            void DoSet(string lhs, string rhs)
            {
                _e.Evaluate(_e.Expr(_e.Sym("Set"), _e.Sym(lhs), _e.Sym(rhs)));
            }

            // b = x
            DoSet("b", "x");
            Assert.AreEqual(_e.Sym("x"), ValOf("b"));

            // a = b
            DoSet("a", "b");
            Assert.AreEqual(_e.Sym("x"), ValOf("a"));

            // b = y (* a should stay == x)
            DoSet("b", "y");
            Assert.AreEqual(_e.Sym("x"), ValOf("a"));
            Assert.AreEqual(_e.Sym("y"), ValOf("b"));
        }

        [Test]
        public void TestSetDelayedOwnValue()
        {
            void DoSet(string lhs, string rhs)
            {
                _e.Evaluate(_e.Expr(_e.Sym("SetDelayed"), _e.Sym(lhs), _e.Sym(rhs)));
            }

            // b := x
            DoSet("b", "x");
            Assert.AreEqual(_e.Sym("x"), ValOf("b"));

            // a := b
            DoSet("a", "b");
            Assert.AreEqual(_e.Sym("x"), ValOf("a"));

            // b := y (* a should now evaluate to y)
            DoSet("b", "y");
            Assert.AreEqual(_e.Sym("y"), ValOf("a"));
            Assert.AreEqual(_e.Sym("y"), ValOf("b"));
        }

        [Test]
        public void TestSetDelayedDownValue()
        {
            Value fOf(params Value[] v)
            {
                return _e.Expr(_e.Sym("f"), v);
            }

            // f[ Blank[] ] := x
            _e.Evaluate(_e.Expr(
                _e.Sym("SetDelayed"),
                fOf(_e.Expr(_e.Sym("Blank"))),
                _e.Sym("x")
            ));

            // f[ y ] == x
            var e1 = _e.Evaluate(fOf(_e.Sym("y")));
            Assert.AreEqual(_e.Sym("x"), e1);

            // f[ y, z ] == f[ y, z ]
            var e2 = fOf(_e.Sym("y"), _e.Sym("z"));
            var e3 = _e.Evaluate(e2);
            Assert.AreEqual(e2, e3);
        }

        [Test]
        public void TestSetDelayedDownValueWithArgs()
        {
            Value fOf(params Value[] v)
            {
                return _e.Expr(_e.Sym("f"), v);
            }

            // f[ ___, x_ ] := x
            // f[ BlankNullSequence[], Pattern[x, Blank[]] ] := x
            _e.Evaluate(_e.Expr(
                _e.Sym("SetDelayed"),
                fOf(
                    _e.Expr(_e.Sym("BlankNullSequence")),
                    _e.Expr(_e.Sym("Pattern"),
                        _e.Sym("x"),
                        _e.Expr(_e.Sym("Blank"))
                    )
                ),
                _e.Sym("x")
            ));

            // f[ y ] == y
            var e1 = _e.Evaluate(fOf(_e.Sym("y")));
            Assert.AreEqual(_e.Sym("y"), e1);

            // f[ y, z ] == z
            var e2 = fOf(_e.Sym("y"), _e.Sym("z"));
            var e3 = _e.Evaluate(e2);
            Assert.AreEqual(_e.Sym("z"), e3);
        }
    }
}
