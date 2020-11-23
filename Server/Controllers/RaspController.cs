using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RaspController : ControllerBase
    {
        private IChatManager _chatManager;

        public RaspController(IChatManager chatManager)
        {
            _chatManager = chatManager;
        }

        private bool value = true;
        

        [HttpGet]
        public ContentResult Get()
        {
            string text;
            var fileStream = new FileStream(@"RaspPage.html", FileMode.Open, FileAccess.Read);
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
        [HttpGet("Text")]
        public string GetText()
        {
            return "Hey Rasppbery";
        }
        [HttpGet("Bool")]
        public bool GetBool()
        {
            return value;
        }
        [HttpPost("Set_Bool")]
        public void Set_Bool(string input)
        {
            if (input == "1") value = true;
            else if (input == "0") value = false;
            else { }
        }
    }
}