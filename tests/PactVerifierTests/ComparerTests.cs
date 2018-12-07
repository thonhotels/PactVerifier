using System;
using System.IO;
using Thon.Hotels.PactVerifier;
using Xunit;

namespace PactVerifierTests
{
    public class ComparerTests
    {
        [Fact]
        public void Int10EqualsInt10()
        {
            var result = Comparer.Compare(10, 10);
            Assert.Empty(result);
        }

        [Fact]
        public void Int10NotEqualsInt11()
        {
            var result = Comparer.Compare(10, 11);
            Assert.Collection(result, s => s.Equals("10 != 11"));
        }

        [Fact]
        public void EmptyStringEqualsNull()
        {
            var result = Comparer.Compare("", null);
            Assert.Collection(result, s => s.Equals("Empty != null"));
        }
    }
}