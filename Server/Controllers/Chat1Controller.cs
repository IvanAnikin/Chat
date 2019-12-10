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
        public async Task<List<string>> GetAllChatsAsync()
        {
            return await _chatManager.GetChatsAsync();
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

            _chatManager.StoreMessageAsync(chatName, message, guid);
        }

        [HttpPost ("CreateChat")]
        public void CreateChat(string chatName)
        {
            _chatManager.CreateChat(chatName);
        }

        //DB
        [HttpGet("GetTableData")]
        public async Task<string> TableGetdata(string chatName)
        {
            return await _chatManager.TableGetData(chatName);
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
        [HttpGet("DBStoreMessage")]
        public async Task<string> DBStoreMessage(string text, string nickname, string chatName)
        {
            return await _chatManager.DBStoreMessage(text, nickname, chatName);
        }
        [HttpGet("DBGetAllChats")]
        public async Task<List<string>> DBGetAllChats()
        {
            return await _chatManager.DBGetAllChats();
        }
        [HttpGet("DBTESTgetmessages")]
        public List<Message> DBGetChatsMessages(string chatName)
        {
            return _chatManager.DBGetChatsMessages(chatName);
        }


        //AUTENTIFICATION 

        [HttpGet("NewUser")]
        public Task<string> Post(string login, string hash, string nickname)
        {
            if (nickname == "") nickname = "Anonymous";

            string level = "administrator";
            //string level = "spectator";
            //string level = "member";

            return _chatManager.DBStoreUser(login, hash, nickname, level);
        }

        [HttpGet("getUserTableTest")]
        public List<UserTable> GetUserTableTest()
        {
            return _chatManager.GetUserTableTest();
        }
    }
}