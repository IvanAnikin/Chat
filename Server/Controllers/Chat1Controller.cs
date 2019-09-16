﻿using System;
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
    public class Chat1Controller : ControllerBase
    {
        private IChatManager _chatManager;

        public Chat1Controller(IChatManager chatManager)
        {
            _chatManager = chatManager;
        }

        [HttpGet]
        public ContentResult Get()
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

        /*[HttpGet("GetLast")]
        public NewSessionResult GetLast(string chatName)
        {
            return _chatManager.GetLastMessages(chatName, 10);  
        }
        */
        [HttpGet("GetAllChats")]
        public List<String> GetAllChats()
        {
            return _chatManager.GetChats();
        }

        [HttpGet("GetNew")]
        public async Task<Message> GetNew(string sessionId, string chatName)
        {
            Message message = await _chatManager.GetNewMessageAsync(chatName, sessionId);

            return message;
        }

        [HttpPost("SendMessage")]
        public void Post(string text, string nickname, string chatName)
        {
            Message message = new Message
            {
                time = DateTime.UtcNow.ToLongTimeString(),
                authorNickName = nickname,
                body = text
            };

            _chatManager.StoreMessage(chatName, message);
        }

        [HttpPost ("CreateChat")]
        public void CreateChat(string chatName)
        {
            _chatManager.CreateChat(chatName);
        }
    }
}