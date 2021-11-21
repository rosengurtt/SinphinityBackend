using NUnit.Framework;
using SinphinitySysStore.Models;

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
    }
}