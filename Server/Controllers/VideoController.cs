using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        [HttpGet]
        public ContentResult Get()
        {

            string text;
            var fileStream = new FileStream(@"VideoPage.html", FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                text = streamReader.ReadToEnd();
            }

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = text
            };
        }

        [HttpGet("face-api")]
        public ContentResult FaceApi()
        {
            StreamReader streamReader = new StreamReader(@"face-api.min.js");
            string script = streamReader.ReadToEnd();
            streamReader.Close();

            return Content(script);

        }

        [HttpGet("string")]
        public string String()
        {
            return "Hey there";

        }


    }
}