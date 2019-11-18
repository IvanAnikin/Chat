﻿using System;
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

    public class MessageTableArray : TableEntity
    {
        private string chatName;
        private string[] message;

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

    public class MessageTableTest : TableEntity
    {
        private string time;
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
            this.PartitionKey = body;
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
            this.RowKey = time;
        }
        public void AssignPartitionKey()
        {
            this.PartitionKey = authorNickName;
        }
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
    }    
}
