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
    public class AutentificationController : ControllerBase
    {
        private IChatManager _chatManager;

        public AutentificationController(IChatManager chatManager)
        {
            _chatManager = chatManager;
        }
        [HttpGet]
        public ContentResult Get()
        {
            string text;
            var fileStream = new FileStream(@"Autentification.html", FileMode.Open, FileAccess.Read);
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

        [HttpGet("NewUser")]
        public Task<string> NewUser(string login, string hash, string nickname)
        {
            if (nickname == "") nickname = "Anonymous";

            string level = "administrator";
            //string level = "spectator";
            //string level = "member";

            return _chatManager.DBStoreUser(login, hash, nickname, level);
        }
        [HttpPost("NewUserPost")]
        public void NewUserPost(string login, string hash, string nickname)
        {
            if (nickname == "") nickname = "Anonymous";

            string level = "administrator";
            //string level = "spectator";
            //string level = "member";

            _chatManager.DBStoreUser(login, hash, nickname, level);
        }

        [HttpGet("getUserTableTest")]
        public List<UserTable> GetUserTableTest()
        {
            return _chatManager.GetUserTableTest();
        }
        [HttpGet("deleteAllUsersTest")]
        public Task<string> DeleteAllUsersTest()
        {
            return _chatManager.DeleteAllUsersTestAsync();
        }
    }
}