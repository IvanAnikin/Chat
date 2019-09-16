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
    public class DefaultController : ControllerBase
    {
        private IChatManager _chatManager;

        public DefaultController(IChatManager chatManager)
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

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = text
            };
        }

        [HttpGet("OnLoad")]
        public NewSessionResultChats OnLoad() => _chatManager.GetChatsSession(10);

        [HttpGet("GetNewChat")]
        public Task<string> GetNewChat(string sessionId) => _chatManager.GetNewChatAsync(sessionId);

        [HttpPost("CreateChat")]
        public void CreateChat(string chatName) => _chatManager.CreateChat(chatName);

        [HttpPost("DeleteChats")]
        public void DeleteChats() => _chatManager.DeleteChats();

        [HttpPost("DeleteChat")]
        public void DeleteChat(string chatName) => _chatManager.DeleteChat(chatName);

        [HttpPost("BBCremove")] //remove bufferBlockChats /when leaving default page
        public void BBCremove(string sessionId) => _chatManager.BBCremove(sessionId);
    }
}