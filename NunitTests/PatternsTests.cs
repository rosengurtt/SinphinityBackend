using NUnit.Framework;
using Sinphinity.Models;

namespace NunitTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void IsMonotoneIsCalculatedCorrectly()
        {
            var asString = "(144,0)(192,0)(96,-4)(96,1)(240,1)(432,-2)(96,0)(144,1)(96,3)(144,0)(192,0)(96,-4)(192,1)(192,1)";
            var pat = new Pattern(asString);
            Assert.IsFalse(pat.IsMonotone);
        }

        [Test]
        public void BasicPatternIsExtractedCorrectlyFromPattern()
        {
            var asString = "(144,0)(192,0)(96,-4)(96,1)(240,1)(432,-2)(96,0)(144,1)(96,3)(144,0)(192,0)(96,-4)(192,1)(192,1)";
            var pat = new Pattern(asString);
            var basPat = new BasicPattern(pat);
            Assert.AreEqual("(3,0)(4,0)(2,-4)(2,1)(5,1)(9,-2)(2,0)(3,1)(2,3)(3,0)(4,0)(2,-4)(4,1)(4,1)", basPat.AsString);
        }
    }
}