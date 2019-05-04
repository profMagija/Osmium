using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using Osmium.Engine.Patterns;
using Osmium.Engine.Values;

namespace Osmium.Engine.Test
{
    public class MatcherTests
    {
        private OsmiumEngine _engine;
        private Value _blank;
        private Value _blankSeq;
        private Value _blankNullSeq;

        [SetUp]
        public void Setup()
        {
            _engine = new OsmiumEngine();
            _blank = _engine.Expr(_engine.Sym("System`Blank"));
            _blankSeq = _engine.Expr(_engine.Sym("System`BlankSequence"));
            _blankNullSeq = _engine.Expr(_engine.Sym("System`BlankNullSequence"));
        }

        [Test]
        public void TestBlankPattern()
        {
            // Blank[x]
            var form1 = _engine.Expr(_engine.Sym("Blank"), _engine.Sym("x"));

            // Blank[]
            var form2 = _blank;

            // x[]
            var expr1 = _engine.Expr(_engine.Sym("x"));

            // y[]
            var expr2 = _engine.Expr(_engine.Sym("y"));

            // MatchQ[ x[], _x ] -> True
            Assert.True(_engine.IsMatch(form1, expr1), "MatchQ[ x[], _x ]");

            // MatchQ[ x[], _ ] -> True
            Assert.True(_engine.IsMatch(form2, expr1), "MatchQ[ x[], _ ]");

            // MatchQ[ y[], _x ] -> False
            Assert.False(_engine.IsMatch(form1, expr2), "MatchQ[ y[], _x ]");
        }

        [Test]
        public void TestExprPattern()
        {
            // a[_, y]
            var form = _engine.Expr(_engine.Sym("a"), _blank, _engine.Sym("y"));

            // a[x, y]
            var expr1 = _engine.Expr(_engine.Sym("a"), _engine.Sym("x"), _engine.Sym("y"));

            // a[x, z]
            var expr2 = _engine.Expr(_engine.Sym("a"), _engine.Sym("x"), _engine.Sym("z"));

            Assert.True(_engine.IsMatch(form, expr1), "MatchQ[ a[x, y], a[_, y] ]");
            Assert.False(_engine.IsMatch(form, expr2), "MatchQ[ a[x, z], a[_, y] ]");
        }

        [Test]
        public void TestExtraInSeqPattern()
        {
            // a[_]
            var form = _engine.Expr(_engine.Sym("a"), _blank);

            // a[]
            var expr1 = _engine.Expr(_engine.Sym("a"));
            // a[x]
            var expr2 = _engine.Expr(_engine.Sym("a"), _engine.Sym("x"));
            // a[x, x]
            var expr3 = _engine.Expr(_engine.Sym("a"), _engine.Sym("x"), _engine.Sym("x"));

            Assert.False(_engine.IsMatch(form, expr1), "MatchQ[ a[], a[_] ]");
            Assert.True(_engine.IsMatch(form, expr2), "MatchQ[ a[x], a[_] ]");
            Assert.False(_engine.IsMatch(form, expr3), "MatchQ[ a[x, x], a[_] ]");
        }

        [Test]
        public void TestBlankSequencePattern()
        {
            // a[__]
            var form = _engine.Expr(_engine.Sym("a"), _blankSeq);

            // a[]
            var expr1 = _engine.Expr(_engine.Sym("a"));
            // a[x]
            var expr2 = _engine.Expr(_engine.Sym("a"), _engine.Sym("x"));
            // a[x, x]
            var expr3 = _engine.Expr(_engine.Sym("a"), _engine.Sym("x"), _engine.Sym("x"));

            Assert.False(_engine.IsMatch(form, expr1), "MatchQ[ a[], a[__] ]");
            Assert.True(_engine.IsMatch(form, expr2), "MatchQ[ a[x], a[__] ]");
            Assert.True(_engine.IsMatch(form, expr3), "MatchQ[ a[x, x], a[__] ]");
        }

        [Test]
        public void TestBlankNullSequencePattern()
        {
            // a[__]
            var form = _engine.Expr(_engine.Sym("a"), _blankNullSeq);

            // a[]
            var expr1 = _engine.Expr(_engine.Sym("a"));
            // a[x]
            var expr2 = _engine.Expr(_engine.Sym("a"), _engine.Sym("x"));
            // a[x, x]
            var expr3 = _engine.Expr(_engine.Sym("a"), _engine.Sym("x"), _engine.Sym("x"));

            Assert.True(_engine.IsMatch(form, expr1), "MatchQ[ a[], a[___] ]");
            Assert.True(_engine.IsMatch(form, expr2), "MatchQ[ a[x], a[___] ]");
            Assert.True(_engine.IsMatch(form, expr3), "MatchQ[ a[x, x], a[___] ]");
        }

        [Test]
        public void TestMultipleSeq()
        {
            // a[___, _]
            var form = _engine.Expr(_engine.Sym("a"), _blankNullSeq, _blank);

            // a[x, x]
            var expr1 = _engine.Expr(_engine.Sym("a"), _engine.Sym("x"), _engine.Sym("x"));

            Assert.True(_engine.IsMatch(form, expr1), "MatchQ[ a[x, x], a[___, _] ]");
        }
    }
}