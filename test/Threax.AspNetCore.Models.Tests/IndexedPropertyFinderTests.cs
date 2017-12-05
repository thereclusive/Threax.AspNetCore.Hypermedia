using System;
using System.Collections.Generic;
using Xunit;

namespace Threax.AspNetCore.Models.Tests
{
    public class IndexedPropertyFinderTests
    {
        class SimEntity
        {
            public List<String> NoIndex { get; set; }

            [IndexProp]
            public List<String> WithIndex { get; set; }
        }

        //Simulate a dbcontext using lists instead, should be the same to the reflection api
        class IndexAttributeFinderTest
        {
            public List<SimEntity> NoIndex { get; set; }
        }

        [Fact]
        public void Test1()
        {
            var propFinder = new IndexedPropertyFinder(typeof(IndexAttributeFinderTest), new Type[] { typeof(List<>) });
            var attributes = propFinder.GetIndexProps();
            var attrEnumerator = attributes.GetEnumerator();
            attrEnumerator.MoveNext();

            //NoIndex should be skipped

            //WithIndex
            Assert.Equal(typeof(SimEntity), attrEnumerator.Current.Type);
            Assert.Equal("WithIndex", attrEnumerator.Current.PropertyInfo.Name);
        }
    }
}
