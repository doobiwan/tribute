using System;
using BookSleeve;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tribute.test
{
    [TestClass]
    public class ExchangeRequestTest
    {
        [TestMethod]
        public void TestExecuteAsync()
        {

            using (var conn = new RedisConnection("localhost"))
            {
                var request = new ExchangeRequest(conn);

                var response = request.ExecuteAsync<TestClass, TestClass>(new TestClass());
                response.Wait();

                Assert.IsNotNull(response.Result);
                Assert.IsInstanceOfType(response.Result, typeof(TestClass));
            }
        }

        class TestClass
        {
            public string Foo = "Bar";
        }
    }
}
