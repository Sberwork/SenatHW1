using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SHW1.Models;

namespace SHW1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {

        [HttpGet("{id}")]
        public ActionResult<Employee> GetById(int id)
        {
            var employee = new Employee()
            {
                ID = id,
                FirstName = "Halim",
                LastName = "Hamidov",
                DateToday = DateTime.Now.ToLocalTime()
            };

            return Ok(employee);
        }
    }
}