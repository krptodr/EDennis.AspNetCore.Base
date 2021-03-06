﻿using EDennis.AspNetCore.Base.Web;
using EDennis.Samples.Hr.ExternalApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace EDennis.Samples.Hr.ExternalApi.Controllers {

    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase {

        InternalApi1 _api1;
        InternalApi2 _api2;

        public EmployeeController(InternalApi1 api1,InternalApi2 api2) {
            _api1 = api1;
            _api2 = api2;
        }



        [HttpPost]
        public ActionResult CreateEmployee([FromBody] Employee employee){
            return _api1.CreateEmployee(employee);
        }


        [HttpGet("{id}")]
        public ActionResult<Employee> GetEmployee([FromRoute] int id) {
            return _api1.GetEmployee(id);
        }


        [HttpGet]
        public ActionResult<List<Employee>> GetEmployees(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 1000) {
            return _api1.GetEmployees(pageNumber, pageSize);
        }


        [HttpPost("agencyonlinecheck")]
        public ActionResult CreateAgencyOnlineCheck(
            [FromBody] AgencyOnlineCheck check) {
            return _api2.CreateAgencyOnlineCheck(check);
        }


        [HttpPost("agencyinvestigatorcheck")]
        public ActionResult CreateAgencyInvestigatorCheck(
            [FromBody] AgencyInvestigatorCheck check) {
            return _api2.CreateAgencyInvestigatorCheck(check);
        }

        [HttpGet("preemployment/{id}")]
        public ActionResult<dynamic> GetPreEmploymentChecks([FromRoute] int id) {
            return _api2.GetPreEmploymentChecks(id);
        }

    }
}
