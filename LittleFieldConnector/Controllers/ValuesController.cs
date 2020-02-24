using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LittleFieldConnector;
using static Utilities;

namespace LittleFieldConnector.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<Dictionary<string, object>> Get()
        {
            var ans = new Dictionary<string, dynamic>();

            foreach (var source in InformationSources.DataSources)
            {
                string responseFromServer = RequestData(source.Key);

                foreach (var field in source.Value)
                {
                    ans.Add(field.Key, field.Value(responseFromServer));
                }
            }

            foreach (var source in InformationSources.ListedDataSources)
            {
                string responseFromServer = RequestData(source.Key);
                ans = ans.Union(source.Value(responseFromServer)).ToDictionary(d => d.Key, d => d.Value);
            }
            return ans;
        }
    }
}
