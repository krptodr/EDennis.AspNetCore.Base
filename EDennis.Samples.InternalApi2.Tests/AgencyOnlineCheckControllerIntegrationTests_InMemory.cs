﻿using EDennis.AspNetCore.Base.EntityFramework;
using EDennis.AspNetCore.Testing;
using EDennis.Samples.InternalApi2;
using EDennis.Samples.InternalApi2.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;


namespace EDennis.Samples.InternalApi2.Tests {

    [Collection("Sequential")]
    public class AgencyOnlineCheckControllerIntegrationTests_InMemory {

        private static WebApplicationFactory<Startup> _factory;        
        private static HttpClient _client;

        static AgencyOnlineCheckControllerIntegrationTests_InMemory() {
            _factory = new WebApplicationFactory<Startup>();
        }

        private readonly ITestOutputHelper _output;

        public AgencyOnlineCheckControllerIntegrationTests_InMemory(ITestOutputHelper output) {
            _output = output;
            _client = _factory.CreateClient();
            _client.BaseAddress = new Uri("http://localhost:5000/iapi/agencyonlinecheck");
        }


        [Theory]
        [InlineData(1, "2018-12-01", "Pass")]
        [InlineData(2, "2018-12-02", "Fail")]
        [InlineData(3, "2018-12-03", "Pass")]
        [InlineData(4, "2018-12-04", "Fail")]
        public void TestCreateAgencyOnlineCheck(int employeeId, string strDateCompleted, string status) {

            var dbName = Guid.NewGuid().ToString();

            var actual = _client.PostAndGet(
                new AgencyOnlineCheck { EmployeeId = employeeId, DateCompleted = DateTime.Parse(strDateCompleted), Status = status },
                DbType.InMemory, dbName, new object[] { employeeId });

            Assert.Equal(status, actual.Status);

        }


    }
}