using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace SecuroteckWebApplicationCore.Controllers
{
    
    public class TalkBackController : Controller
    {

        [ActionName("Hello")]
        public IActionResult Get()
        {
            return Ok("Hello World");
        }



        [ActionName("Sort"), HttpGet]
         public IActionResult Get([FromQuery] string[] integers)
        {

            var result = new List<int>();
            foreach (var integer in integers)
            {
                int converted;
                if (int.TryParse(integer, out converted))
                {
                    result.Add(converted);
                }
                else
                {
                    return BadRequest("Bad Request");
                }
            }

            return Ok(Json(result));
        }



    }


}
