using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Server
{
    public class TestJsonJob
    {
        public string sessionId;
        public List<Message> lastMessages;
        public string sas;
    }


    public class NewSessionResult
    {
        public string sessionId;
        public List<Message> lastMessages;
        public string sas;
    }
    public class ResultSignIn
    {
        public string userID;
    }
    public class NewSessionResultChats
    {
        public string sessionId;
        public List<string> chats;
    }

    public class Message
    {
        public string time;
        public string authorNickName;
        public string body;

        public bool isPicture;
    }

    public class MessageTable : TableEntity
    {
        private string time;
        private string authorNickName;
        private string body;

        public string Time
        {
            get
            {
                return time;
            }

            set
            {
                time = value;
            }
        }

        public string AuthorNickName
        {
            get
            {
                return authorNickName;
            }

            set
            {
                authorNickName = value;
            }
        }

        public string Body
        {
            get
            {
                return body;
            }

            set
            {
                body = value;
            }
        }


        public void AssignRowKey()
        {
            this.RowKey = time;
        }
        public void AssignPartitionKey()
        {
            this.PartitionKey = authorNickName;
        }
    }

    public class UserTable : TableEntity
    {
        private string login;
        private string hash;
        private string nickname;
        private string level;
        private string photo;

        public string Login
        {
            get
            {
                return login;
            }

            set
            {
                login = value;
            }
        }

        public string Hash
        {
            get
            {
                return hash;
            }

            set
            {
                hash = value;
            }
        }

        public string Nickname
        {
            get
            {
                return nickname;
            }

            set
            {
                nickname = value;
            }
        }

        public string Photo
        {
            get
            {
                return photo;
            }
            set
            {
                photo = value;
            }
        }

        public string Level
        {
            get
            {
                return level;
            }
            set
            {
                level = value;
            }
        }


        public void AssignRowKey()
        {
            this.RowKey = login;
        }
        public void AssignPartitionKey()
        {
            this.PartitionKey = hash;
        }
    }

    public class ActiveUserTable : TableEntity
    {
        private string login;
        private string userID;
        private DateTime timeStart;


        public DateTime TimeStart
        {
            get
            {
                return timeStart;
            }

            set
            {
                timeStart = value;
            }
        }

        public string Login
        {
            get
            {
                return login;
            }

            set
            {
                login = value;
            }
        }
        public string UserID
        {
            get
            {
                return userID;
            }

            set
            {
                userID = value;
            }
        }

        public void AssignRowKey()
        {
            this.RowKey = login;
        }
        public void AssignPartitionKey()
        {
            this.PartitionKey = userID;
        }
    }

    public class Zadani
    {
        public string zadani;
        public string zadaniMensi;
        public string reseni1;
        public string reseni2;
        public string napoveda1;
        public string napoveda2;
        public string napoveda3;
    }

    public class Misto
    {
        public string nazev;
        public string popis;
    }

    public class Vitezove
    {
        public String[] seznam_jmen;
        public String[] seznam_prijmeni;
    }

    public interface IChatManager
    {
        Task StoreMessageAsync(string chatName, Message message, string guid);
        NewSessionResult GetLastMessages(string chatName, int count);
        Task<NewSessionResultChats> GetChatsSessionAsync(int count);
        Task<Message> GetNewMessageAsync(string chatName, string sessionId);
        Task<string> GetNewChatAsync(string sessionId);
        Task<List<string>> GetChatsAsync();
        void CreateChat(string chatName);
        void DeleteChats();
        Task DeleteChatAsync(string chatName);
        void BBCremove(string sessionId);
        void BBremove(string sessionId);
        Task<string> TableGetData(string ChatName);
        Task<string> DBCreateNewChat(string chatName);
        Task<string> DBDeleteChat(string chatName);
        Task<string> DBStoreMessage(string text, string nickname, string chatName);
        Task<List<string>> DBGetAllChats();
        List<Message> DBGetChatsMessages(string chatName);
        Task<string> CreateNewBlobContainerTestAsync(string chatName);
        string GetSasTest();
        string GetVisKeyTest();
        Message GetLastMessageTest(string chatName, int count);
        List<MessageTable> GetMessageTableTest(string chatName);
        List<UserTable> GetUserTableTest(string tableName);
        Task<string> DBStoreUser(string login, string hash, string nickname, string level);
        Task<string> DeleteAllUsersTestAsync();
        Task<bool> CheckCredentialsAsync(string login, string hash);
        Task<bool> CheckActiveUserIDsAsync(string activeUserID, string login);
        Task<ResultSignIn> GetResultLoginAsync(string login, string password);
        Task<string> ChangeUserPicture(string login, string hash, string pictureName);
        Task<string> ChangeUserPictureTest(string login, string hash, string pictureName);
        Task<UserTable> GetUserByLogin(string login);
        Task<string> ChangeUserPictureNew(string login, string pictureName);
        Task<string> ChangeUserNickname(string login, string nickname);


        Task<bool> SifrovackaGetResultLoginAsync(string name, string surname);
        Task<string> SifrovackaNavigaceGetLocation(string name, string surname);
        Task<Zadani> SifrovackaNavigaceGetZadani(string name, string surname);
        Task<string> SifrovackaNavigaceGetSolution(string name, string surname);
        Task<string> SifrovackaSifraSubmit(string name, string surname);
        Task<Misto> SifrovackaMistoGet(string name, string surname);
        Task<Vitezove> SifrovackaVitezoveGet();
    }    
}
