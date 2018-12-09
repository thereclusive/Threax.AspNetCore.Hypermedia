using System;
using System.Collections.Generic;
using Xunit;

namespace Threax.AspNetCore.Halcyon.Client.Tests
{
    public class QueryBuilderTests
    {
        class SimpleQueryTest
        {
            public String Name { get; set; }

            public int Number { get; set; }
        }

        [Fact]
        public void BasicQuery()
        {
            var query = QueryBuilder.BuildQueryString(new SimpleQueryTest()
            {
                Name = "Bob",
                Number = 1
            });
            Assert.Equal("Name=Bob&Number=1", query);
        }

        [Fact]
        public void EncodedQuery()
        {
            var query = QueryBuilder.BuildQueryString(new SimpleQueryTest()
            {
                Name = "Bob Smith & The Crew / Other Peeps",
                Number = 1
            });
            Assert.Equal("Name=Bob%20Smith%20&%20The%20Crew%20/%20Other%20Peeps&Number=1", query);
        }

        [Fact]
        public void QueryWithNull()
        {
            var query = QueryBuilder.BuildQueryString(new SimpleQueryTest()
            {
                Name = null,
                Number = 1
            });
            Assert.Equal("Number=1", query);
        }

        class ArrayQueryTest
        {
            public String Name { get; set; }

            public int[] Numbers { get; set; }
        }

        [Fact]
        public void ArrayQuery()
        {
            var query = QueryBuilder.BuildQueryString(new ArrayQueryTest()
            {
                Name = "Bob",
                Numbers = new int[] { 1, 15, 20 }
            });
            Assert.Equal("Name=Bob&Numbers=1&Numbers=15&Numbers=20", query);
        }

        [Fact]
        public void ArrayQueryOneItem()
        {
            var query = QueryBuilder.BuildQueryString(new ArrayQueryTest()
            {
                Name = "Bob",
                Numbers = new int[] { 1 }
            });
            Assert.Equal("Name=Bob&Numbers=1", query);
        }

        [Fact]
        public void DictionaryQuery()
        {
            var query = QueryBuilder.BuildQueryString(new Dictionary<String, Object>()
            {
                { "Name", "Bob" },
                { "Numbers", new List<int>() { 1, 15, 20 } }
            });
            Assert.Equal("Name=Bob&Numbers=1&Numbers=15&Numbers=20", query);
        }
    }
}
