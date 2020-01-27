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
    public class ChatController : ControllerBase
    {
        private IChatManager _chatManager;

        public ChatController(IChatManager chatManager)
        {
            _chatManager = chatManager;
        }
        

        [HttpGet]
        public ContentResult Get(string chatName)
        {

            string text;
            var fileStream = new FileStream(@"DefaultChatPage.html", FileMode.Open, FileAccess.Read);
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

        [HttpGet("LoadChat")]
        public NewSessionResult LoadChat(string chatName) => _chatManager.GetLastMessages(chatName, 10);

        [HttpGet("GetNew")]
        public async Task<Message> GetNew(string sessionId, string chatName) => await _chatManager.GetNewMessageAsync(chatName, sessionId);

        [HttpPost("SendMessage")]
        public void Post(string text, string nickname, string chatName, string guid, string isPicture)
        {

            Message message = new Message
            {
                time = DateTime.UtcNow.ToLongTimeString(),
                authorNickName = nickname,
                body = text,
                isPicture = false
            };

            if (isPicture == "true") message.isPicture = true;
            else message.isPicture = false;

            if (nickname == "") message.authorNickName = "Anonymous";

            _chatManager.StoreMessageAsync(chatName, message, guid);
        }

        [HttpPost("BBremove")] //remove bufferBlockChats /when leaving default page
        public void BBremove(string sessionId) => _chatManager.BBremove(sessionId);

        [HttpGet("GetSasTest")]
        public string GetSasTest()
        {
            return _chatManager.GetSasTest();
        }
        [HttpGet("GetVisKeyTest")]
        public string GetVisKeyTest()
        {
            return _chatManager.GetVisKeyTest();
        }
    }
}