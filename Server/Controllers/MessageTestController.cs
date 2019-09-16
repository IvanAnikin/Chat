using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageTestController : ControllerBase
    {
        private IChatManager _chatManager;

        public MessageTestController(IChatManager chatManager)
        {
            _chatManager = chatManager;
        }

        [HttpGet]
        public ContentResult Get()
        {
            string text;
            var fileStream = new FileStream(@"DefaultPage.html", FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                text = streamReader.ReadToEnd();
            }

            //MESSAGE
            /*
            Message message = new Message();
            message.time = DateTime.UtcNow.ToLongTimeString();
            message.authorNickName = "testNickname";
            message.body = "testMessage";

            _chatManager.StoreMessage("1", message);
            */
            //MESSAGE

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = text
            };
        }

        [HttpPost]
        public void Post()
        {
            Message message = new Message();
            message.time = DateTime.UtcNow.ToLongTimeString();
            message.authorNickName = "nickname";
            message.body = "message";

            _chatManager.StoreMessage("1", message);
        }
    }
}