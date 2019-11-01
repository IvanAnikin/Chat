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
        public void Post(string text, string nickname, string chatName, string guid)
        {
            Message message = new Message
            {
                time = DateTime.UtcNow.ToLongTimeString(),
                authorNickName = nickname,
                body = text
            };

            _chatManager.StoreMessage(chatName, message, guid);
        }

        [HttpPost ("CreateChat")]
        public void CreateChat(string chatName)
        {
            _chatManager.CreateChat(chatName);
        }

        //table testing

        [HttpGet("TableSend")]
        public async Task<string> TableSendTestAsync(string text, string nickname, string chatName, string connStr)
        {
            return await _chatManager.SendToTableTestAsync(text, nickname, chatName, connStr);
            //return await _chatManager.SendToTableAsync(text, nickname, chatName, connStr);
        }

        [HttpGet("GetTableData")]
        public async Task<string> TableGetdata()
        {
            return await _chatManager.TableGetData();
        }
        [HttpGet("CreateTable")]
        public async Task<string> CreateTable()
        {
            return await _chatManager.CreateNewTables();
        }
        [HttpGet("SendMessageArray")]
        public async Task<string> SendMessagearray(string text, string nickname, string chatName, string connStr)
        {
            return await _chatManager.SendToTableTestArrayAsync(text, nickname, chatName, connStr);
        }
        [HttpGet("DBCreateNewChat")]
        public async Task<string> DBCreateNewChat(string chatName)
        {
            return await _chatManager.DBCreateNewChat(chatName);
        }
        [HttpGet("DBDeleteChat")]
        public async Task<string> DBCDeleteChat(string chatName)
        {
            return await _chatManager.DBDeleteChat(chatName);
        }
    }
}