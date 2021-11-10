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
        [HttpGet("WithId")]
        public async Task<ContentResult> GetWithIDAsync(string userId, string login)
        {

            
            if(await _chatManager.CheckActiveUserIDsAsync(userId, login))
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
            else
            {
                return null;
            }

            
        }
        [HttpGet("userSettings")]
        public ContentResult UsersSettings(string userId, string login)
        {
            string text;
            var fileStream = new FileStream(@"usersSettings.html", FileMode.Open, FileAccess.Read);
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
        [HttpGet("chatSettings")]
        public ContentResult ChatSettings(string chatName)
        {
            string text;
            var fileStream = new FileStream(@"chatSettings.html", FileMode.Open, FileAccess.Read);
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

        [HttpGet("ComputerVision")]
        public ContentResult ComputerVision()
        {
            string text;
            var fileStream = new FileStream(@"mlComVis.html", FileMode.Open, FileAccess.Read);
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

        /*

        [HttpGet("Sifrovacka_registrace")]
        public ContentResult Sifrovacka_registrace()
        {
            string text;
            var fileStream = new FileStream(@"sifrovacka_registrace.html", FileMode.Open, FileAccess.Read);
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
        [HttpGet("Sifrovacka")]
        public ContentResult Sifrovacka()
        {
            string text;
            var fileStream = new FileStream(@"sifrovacka.html", FileMode.Open, FileAccess.Read);
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


        [HttpPut("Sifrovacka_LogIn")]
        public async Task<string> SifrovackaLogIn(string name, string surname)
        {
            bool result = await _chatManager.SifrovackaGetResultLoginAsync(name, surname);

            if (result) return "true";
            else return "false";
        }

        [HttpGet("Sifrovacka_Navigace_GetLocation")]
        public async Task<string> Sifrovacka_Navigace_GetLocation(string name, string surname)
        {
            return await _chatManager.SifrovackaNavigaceGetLocation(name, surname);

        }

        [HttpGet("Sifrovacka_Navigace")]
        public async Task<ContentResult> Navigace()
        {
            //var name = Request.Cookies["name"];
            //var surname = Request.Cookies["surname"];

            //bool result = await _chatManager.SifrovackaGetResultLoginAsync(name, surname);

            //if (result)
            //{
            string text;
            var fileStream = new FileStream(@"sifrovacka.html", FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                text = streamReader.ReadToEnd();
            }

            //text = text
            //       .Replace("[NAME]", name)
            //       .Replace("[SURNAME]", name);

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = text
            };
            //}
            //else
            //{
            //    return null;
            //}
            
        }
        [HttpGet("Sifrovacka_Sifra")]
        public async Task<ContentResult> Sifra()
        {
            string text;
            var fileStream = new FileStream(@"sifrovacka_sifra.html", FileMode.Open, FileAccess.Read);
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
        [HttpGet("Sifrovacka_Sifra_GetZadani")]
        public async Task<Zadani> Sifrovacka_Sifra_GetZadani(string name, string surname)
        {
            return await _chatManager.SifrovackaNavigaceGetZadani(name, surname);

        }
        [HttpGet("Sifrovacka_Sifra_GetSolution")]
        public async Task<string> Sifrovacka_Sifra_GetSolution(string name, string surname)
        {
            return await _chatManager.SifrovackaNavigaceGetSolution(name, surname);

        }
        [HttpPut("Sifrovacka_Sifra_Submit")]
        public async Task<string> SifrovackaSifraSubmit(string name, string surname)
        {
            return await _chatManager.SifrovackaSifraSubmit(name, surname);

        }


        [HttpGet("Sifrovacka_Misto")]
        public async Task<ContentResult> Misto()
        {
            string text;
            var fileStream = new FileStream(@"sifrovacka_misto.html", FileMode.Open, FileAccess.Read);
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
        [HttpGet("Sifrovacka_Misto_Get")]
        public async Task<Misto> Sifrovacka_Misto_Get(string name, string surname)
        {
            return await _chatManager.SifrovackaMistoGet(name, surname);

        }
        
        */
        [HttpGet("Sifrovacka_Vitezove")]
        public async Task<ContentResult> Vitezove()
        {
            string text;
            var fileStream = new FileStream(@"sifrovacka_vitezove.html", FileMode.Open, FileAccess.Read);
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

        [HttpGet("Sifrovacka_Vitezove_Get")]
        public async Task<Vitezove> Sifrovacka_Vitezove_Get()
        {
            return await _chatManager.SifrovackaVitezoveGet();

        }

        [HttpGet("About_Me")]
        public ContentResult AboutMe()
        {
            string text;
            var fileStream = new FileStream(@"about_me.html", FileMode.Open, FileAccess.Read);
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



        [HttpGet("MyProjects")]
        public ContentResult MyProjects()
        {
            string text;
            var fileStream = new FileStream(@"MyProjects.html", FileMode.Open, FileAccess.Read);
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
        [HttpGet("ReinforcementLearning")]
        public ContentResult ReinforcementLearning()
        {
            string text;
            var fileStream = new FileStream(@"ReinforcementLearning.html", FileMode.Open, FileAccess.Read);
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

        [HttpPut("changeUserPicture")]
        public async void changeUserPicture(string userId, string login, string hash, string pictureName)
        {
            if (await _chatManager.CheckActiveUserIDsAsync(userId, login))
            {
                await _chatManager.ChangeUserPicture(login, hash, pictureName);
            }
        }

        [HttpGet("changeUserPictureGet")]
        public async Task<string> changeUserPictureGet(string userId, string login, string pictureName)
        {
            if (await _chatManager.CheckActiveUserIDsAsync(userId, login))
            {
                return await _chatManager.ChangeUserPictureNew(login, pictureName);
                //return "DONE";
            }
            else return "error";
        }
        [HttpGet("changeUserNicknameGet")]
        public async Task<string> changeUserNicknameGet(string userId, string login, string nickname)
        {
            if (await _chatManager.CheckActiveUserIDsAsync(userId, login))
            {
                return await _chatManager.ChangeUserNickname(login, nickname);
                //return "DONE";
            }
            else return "error";
        }
        [HttpGet("GetUserByLogin")]
        public Task<UserTable> GetUserByLogin(string login) => _chatManager.GetUserByLogin(login);

        [HttpGet("OnLoad")]
        public async Task<NewSessionResultChats> OnLoadAsync() => await _chatManager.GetChatsSessionAsync(10);

        [HttpGet("GetNewChat")]
        public Task<string> GetNewChat(string sessionId) => _chatManager.GetNewChatAsync(sessionId);

        [HttpPost("CreateChat")]
        public void CreateChat(string chatName) => _chatManager.CreateChat(chatName);

        [HttpPost("DeleteChats")]
        public void DeleteChats() => _chatManager.DeleteChats();

        [HttpPost("DeleteChat")]
        public void DeleteChat(string chatName) => _chatManager.DeleteChatAsync(chatName);

        [HttpPost("BBCremove")] //remove bufferBlockChats /when leaving default page
        public void BBCremove(string sessionId) => _chatManager.BBCremove(sessionId);


    }
}