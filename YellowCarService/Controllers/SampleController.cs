using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace YellowCar.Controllers
{
    public class SampleController : ApiController
    {
        // GET: api/Sample
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Sample/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Sample
        public async void Post(/*[FromBody]byte[] imageBytes*/)
        {
            byte[] bytes = await Request.Content.ReadAsByteArrayAsync();

            //byte[] bytes = await imageContent.ReadAsByteArrayAsync();
            string content = "Hello World";
        }

        // PUT: api/Sample/5
        public void Put(int id, [FromBody]string value)
        {

        }

        // DELETE: api/Sample/5
        public void Delete(int id)
        {
        }
    }
}
