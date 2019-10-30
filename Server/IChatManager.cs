using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Server
{
    public class NewSessionResult
    {
        public string sessionId;
        public List<Message> lastMessages;
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
    }

    public class MessageTableNew : TableEntity
    {
        private string chatName;
        private string[]  message;

        public string ChatName
        {
            get
            {
                return chatName;
            }

            set
            {
                chatName = value;
            }
        }
        public string[] Message
        {
            get
            {
                return message;
            }

            set
            {
                message = value;
            }
        }
        public void AssignRowKey()
        {
            this.RowKey = message[0];
        }
        public void AssignPartitionKey()
        {
            this.PartitionKey = message[1];
        }
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
            this.RowKey = authorNickName;
        }
        public void AssignPartitionKey()
        {
            this.PartitionKey = time;
        }
    }

    public interface IChatManager
    {
        void StoreMessage(string chatName, Message message, string guid);
        NewSessionResult GetLastMessages(string chatName, int count);
        NewSessionResultChats GetChatsSession(int count);
        Task<Message> GetNewMessageAsync(string chatName, string sessionId);
        Task<string> GetNewChatAsync(string sessionId);
        List<string> GetChats();
        void CreateChat(string chatName);
        void DeleteChats();
        void DeleteChat(string chatName);
        void BBCremove(string sessionId);
        void BBremove(string sessionId);
        Task<string> SendToTableAsync(string text, string nickname, string chatName, string connStr);
    }    
}
