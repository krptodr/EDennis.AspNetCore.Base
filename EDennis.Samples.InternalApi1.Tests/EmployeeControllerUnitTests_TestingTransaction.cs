﻿using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using EDennis.Samples.InternalApi1.Models;
using EDennis.AspNetCore.Testing;
using EDennis.AspNetCore.Base.EntityFramework;
using EDennis.Samples.InternalApi1.Controllers;
using EDennis.NetCoreTestingUtilities.Extensions;
using System.Threading.Tasks;
using EDennis.NetCoreTestingUtilities;

namespace EDennis.Samples.InternalApi1.Tests {

    [Collection("Sequential")]
    public class EmployeeControllerUnitTests_TestingTransaction : TransactionTest<HrContext> {

        private EmployeeController _controller;
        private EmployeeRepo _repo;
        private readonly ITestOutputHelper _output;

        public EmployeeControllerUnitTests_TestingTransaction(ITestOutputHelper output) {
            _repo = new EmployeeRepo(Context);
            _controller = new EmployeeController(_repo);
            _output = output;
        }

        [Theory]
        [InlineData("Regis")]
        [InlineData("Wink")]
        [InlineData("Moe")]
        [InlineData("Larry")]
        [InlineData("Curly")]
        public async Task TestCreateEmployee(string firstName) {
            var max = Context.GetMaxKeyValue<Employee>();
            _output.WriteLine($"max of Employee Id: {max}");
            var response = _controller.CreateEmployee(new Employee { FirstName = firstName });
            var content = response.Result.GetObject<Employee>();
            var result = await _repo.GetByLinqAsync(e => e.FirstName == firstName, 1, 1000);
            var count = result.Count;
            Assert.Equal(1, count);
        }


    }
}