using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CamelsClub.API.Mahmoud
{
    public class testController : ApiController
    {

        [HttpGet]
        [Route("api/Mahmoud/GetData")]
        public IHttpActionResult Get()
        {
            var employess = new List<Employee>{
                new Employee{Id=1,Name="Ahmed" },
                new Employee{Id=2,Name="Mahmoud" },
            };
            return Ok(employess);
        }


        public class Employee
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
