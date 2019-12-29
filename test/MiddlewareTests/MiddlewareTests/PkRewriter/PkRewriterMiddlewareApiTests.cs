﻿using EDennis.AspNetCore.Base;
using EDennis.AspNetCore.Base.Web;
using EDennis.NetCoreTestingUtilities;
using EDennis.NetCoreTestingUtilities.Extensions;
using PkRewriterApi.Tests;
using PkRewriterApi;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace EDennis.AspNetCore.MiddlewareTests {
    /// <summary>
    /// Note: IClassFixture was not used because
    /// the individual test cases were conflicting with each
    /// (possibly, one test case was updating the configuration
    /// while another test case was calling Get).  To resolve
    /// the issue, I instantiate the TestApis within the
    /// test method.  This is inefficient, but it works.
    /// </summary>
    [Collection("Sequential")]
    public class PkRewriterMiddlewareApiTests {

        private readonly ITestOutputHelper _output;

        public PkRewriterMiddlewareApiTests(
            ITestOutputHelper output) {
            _output = output;
        }


        internal class TestJsonA : TestJsonAttribute {
            public TestJsonA(string methodName, string testScenario, string testCase)
                : base("PkRewriterApi", "PersonController",
                      methodName, testScenario, testCase, NetCoreTestingUtilities.DatabaseProvider.Excel, "PkRewriter\\TestJson.xlsx") {
            }
        }


        [Theory]
        [TestJsonA("Create", "", "A")]
        [TestJsonA("Create", "", "B")]
        [TestJsonA("Create", "", "C")]
        public void Create(string t, JsonTestCase jsonTestCase) {

            using var factory = new TestApis();
            var client = factory.CreateClient["PkRewriterApi"]();
            _output.WriteLine($"Test case: {t}");

            //send configuration for test case
            var jcfg = File.ReadAllText($"PkRewriter\\{jsonTestCase.TestCase}.json");
            var status = client.Configure("", jcfg);

            //make sure that configuration was successful
            Assert.Equal((int)System.Net.HttpStatusCode.OK, status.GetStatusCode());

            var url = $"Person";

            var input = jsonTestCase.GetObject<Person>("Input");
            var expected = jsonTestCase.GetObject<List<Person>>("Expected");

            client.Post(url, input);
            var result = client.Get<List<Person>>(url);
            var actual = result.GetObject<List<Person>>();
            Assert.True(actual.IsEqualOrWrite(expected, _output, true));


            var expectedBypass = jsonTestCase.GetObject<List<Person>>("ExpectedBypass");
            var resultBypass = client.Get<List<Person>>($"{url}?{Constants.PK_REWRITER_BYPASS_KEY}=true");
            var actualBypass = resultBypass.GetObject<List<Person>>();
            Assert.True(actualBypass.IsEqualOrWrite(expectedBypass, _output, true));

        }



        [Theory]
        [TestJsonA("Update", "CreateUpdate", "A")]
        [TestJsonA("Update", "CreateUpdate", "B")]
        [TestJsonA("Update", "CreateUpdate", "C")]
        public void CreateUpdate(string t, JsonTestCase jsonTestCase) {

            using var factory = new TestApis();
            var client = factory.CreateClient["PkRewriterApi"]();
            _output.WriteLine($"Test case: {t}");

            //send configuration for test case
            var jcfg = File.ReadAllText($"PkRewriter\\{jsonTestCase.TestCase}.json");
            var status = client.Configure("", jcfg);

            //make sure that configuration was successful
            Assert.Equal((int)System.Net.HttpStatusCode.OK, status.GetStatusCode());

            var url = $"Person";

            var input1 = jsonTestCase.GetObject<Person>("Input1");
            client.Post(url, input1);

            var input2 = jsonTestCase.GetObject<Person>("Input2");
            var id = jsonTestCase.GetObject<int>("Id");
            var expected = jsonTestCase.GetObject<List<Person>>("Expected");

            client.Put($"{url}/{id}", input2);
            var result = client.Get<List<Person>>(url);
            var actual = result.GetObject<List<Person>>();
            Assert.True(actual.IsEqualOrWrite(expected, _output, true));

            var resultBypass = client.Get<List<Person>>($"{url}?{Constants.PK_REWRITER_BYPASS_KEY}=true");
            var actualBypass = resultBypass.GetObject<List<Person>>();
            var expectedBypass = jsonTestCase.GetObject<List<Person>>("ExpectedBypass");
            Assert.True(actualBypass.IsEqualOrWrite(expectedBypass, _output, true));

        }


        [Theory]
        [TestJsonA("Update", "Update", "A")]
        [TestJsonA("Update", "Update", "B")]
        [TestJsonA("Update", "Update", "C")]
        public void Update(string t, JsonTestCase jsonTestCase) {

            using var factory = new TestApis();
            var client = factory.CreateClient["PkRewriterApi"]();
            _output.WriteLine($"Test case: {t}");

            //send configuration for test case
            var jcfg = File.ReadAllText($"PkRewriter\\{jsonTestCase.TestCase}.json");
            var status = client.Configure("", jcfg);

            //make sure that configuration was successful
            Assert.Equal((int)System.Net.HttpStatusCode.OK, status.GetStatusCode());

            var url = $"Person";

            var input = jsonTestCase.GetObject<Person>("Input");
            var id = jsonTestCase.GetObject<int>("Id");
            var expected = jsonTestCase.GetObject<int>("Expected");

            client.Put($"{url}/{id}", input);
            var result = client.Get<List<Person>>(url);
            var actual = result.GetStatusCode();
            Assert.True(actual.IsEqualOrWrite(expected, _output, true));

        }


        [Theory]
        [TestJsonA("Query", "", "A")]
        [TestJsonA("Query", "", "B")]
        [TestJsonA("Query", "", "C")]
        public void Query(string t, JsonTestCase jsonTestCase) {

            using var factory = new TestApis();
            var client = factory.CreateClient["PkRewriterApi"]();
            _output.WriteLine($"Test case: {t}");

            //send configuration for test case
            var jcfg = File.ReadAllText($"PkRewriter\\{jsonTestCase.TestCase}.json");
            var status = client.Configure("", jcfg);

            //make sure that configuration was successful
            Assert.Equal((int)System.Net.HttpStatusCode.OK, status.GetStatusCode());

            var url = $"Person";

            var input1 = jsonTestCase.GetObject<Person>("Input1");
            var input2 = jsonTestCase.GetObject<Person>("Input2");
            client.Post(url, input1);
            client.Post(url, input2);

            var queryString = jsonTestCase.GetObject<string>("QueryString");
            var expected = jsonTestCase.GetObject<List<Person>>("Expected");

            client.Get<List<Person>>($"{url}?{queryString}");
            var result = client.Get<List<Person>>(url);
            var actual = result.GetObject<List<Person>>();
            Assert.True(actual.IsEqualOrWrite(expected, _output, true));


        }



    }
}
