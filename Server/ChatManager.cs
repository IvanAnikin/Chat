using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Server
{

    public class ChatManager : IChatManager
    {
        private ConcurrentDictionary<string, List<Message>> _chats = new ConcurrentDictionary<string, List<Message>>();
        private ConcurrentDictionary<string, List<string>> _chatSessions = new ConcurrentDictionary<string, List<string>>();
        private ConcurrentDictionary<string, BufferBlock<Message>> _sessionListeners = new ConcurrentDictionary<string, BufferBlock<Message>>();
        private ConcurrentDictionary<string, BufferBlock<string>> _bufferBlocksChat = new ConcurrentDictionary<string, BufferBlock<string>>();

        private readonly string connectionString;

        private CloudStorageAccount storageAccount = null;
        CloudTableClient tableClient = null;

        public ChatManager(string connectionString)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException("ConnectionString is null");

            if (CloudStorageAccount.TryParse(connectionString, out storageAccount))
            {
                tableClient = storageAccount.CreateCloudTableClient();
            }
            else
            {
                throw new ArgumentNullException("wrong connection string");
            }

        }

        public async Task StoreMessageAsync(string chatName, Message message, string guid)
        {
            if(!_chats.ContainsKey(chatName))
            {
                _chats[chatName] = new List<Message>(); 
            }
            _chats[chatName].Add(message);
            //DB
            await DBStoreMessage(message.body, message.authorNickName, chatName);
            //DB
            if (_chatSessions.TryGetValue(chatName, out var sessionIds))
            {
                foreach (var sessionId in sessionIds)
                {
                    await _sessionListeners[sessionId].SendAsync(message);
                }
            }
        }
        
        public NewSessionResult GetLastMessages(string chatName, int count)
        {
            //await CreateNewTables(); //DB

            string guid = Guid.NewGuid().ToString();
            _sessionListeners[guid] = new BufferBlock<Message>();
            if (!_chatSessions.ContainsKey(chatName))
            {
                _chatSessions[chatName] = new List<string>();
            }
            _chatSessions[chatName].Add(guid);

            List<Message> outputMessages = new List<Message> { };

            try
            {
                var messages = _chats[chatName];

                if (messages.Count <= 10)
                {
                    outputMessages = messages;
                }
                else
                {
                    for (int i = 0; i < 10; i++)
                    {
                        outputMessages.Add(messages[messages.Count - 10 + i]);
                    }
                }
            }
            catch
            {
                
            }
            return new NewSessionResult { sessionId = guid, lastMessages = outputMessages};
        }

        public NewSessionResultChats GetChatsSession(int count)
        {
            string guid = Guid.NewGuid().ToString();
            _bufferBlocksChat[guid] = new BufferBlock<string>();

            List<string> chats = new List<string>();
            foreach (var chat in _chats)
            {
                chats.Add(chat.Key);
            }

            return new NewSessionResultChats { sessionId = guid, chats = chats };
        }

        public async Task<Message> GetNewMessageAsync(string chatName, string sessionId)
        {
            return await _sessionListeners[sessionId].ReceiveAsync();
        }

        public async Task<string> GetNewChatAsync(string sessionId)
        {
            return await _bufferBlocksChat[sessionId].ReceiveAsync();
        }

        public List<string> GetChats()
        {
            List<string> chats = new List<string>();
            foreach(var chat in _chats)
            {
                chats.Add(chat.Key);
            }
            return chats;
        }

        public async void CreateChat(string chatName)
        {
            // DB 
            if (CloudStorageAccount.TryParse(connectionString, out storageAccount))
            {
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                CloudTable cloudTable = tableClient.GetTableReference(chatName);
                await CreateNewTableAsync(cloudTable);
            }
            //DB 

            if (!_chats.ContainsKey(chatName)) // look if there is this chat in DB 
            {
                _chats[chatName] = new List<Message>();

                foreach (var bufferBlock in _bufferBlocksChat)
                {
                    await bufferBlock.Value.SendAsync(chatName);
                }
            }
        }

        public void DeleteChats()
        {
            _chats.Clear();
        }

        public async Task DeleteChatAsync(string chatName)
        {
            _chats.Remove(chatName, out List<Message> value);
            //DB
            await DBDeleteChat(chatName);
            //DB
        }

        public void BBCremove(string sessionId)
        {
            _bufferBlocksChat.TryRemove(sessionId, out BufferBlock<string> value);
        }

        public void BBremove(string sessionId)
        {
            _sessionListeners.TryRemove(sessionId, out BufferBlock<Message> _);
            foreach(var chat in _chatSessions)
            {
                foreach(var session in chat.Value)
                {
                    if(session == sessionId)
                    {
                        chat.Value.Remove(session);
                        if(chat.Value.Count == 0)
                        {
                            _chatSessions.TryRemove(chat.Key, out var _);
                        }
                        return;
                    }
                }
            }
        }



        //
        //DB TOOLS 
        //

        //create table for NEW CHAT
        public async Task<string> DBCreateNewChat(string chatName)
        {
            if (CloudStorageAccount.TryParse(connectionString, out storageAccount))
            {
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                CloudTable cloudTable = tableClient.GetTableReference(chatName);
                return await CreateNewTableAsync(cloudTable);
            }
            else
            {
                return "wrong connection string";
            }
        }

        //DELETE TABLE of chat
        public async Task<string> DBDeleteChat(string chatName)
        {
            if (CloudStorageAccount.TryParse(connectionString, out storageAccount))
            {
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                //TableOperation tableOperation = TableOperation.Delete("chat");

                CloudTable cloudTable = tableClient.GetTableReference(chatName);

                if (await cloudTable.DeleteIfExistsAsync()) return "deleted";
                else return "no such table";
            }
            else
            {
                return "wrong connection string";
            }
        }

        //STORE MESSAGE in table with name of chatName
        public async Task<string> DBStoreMessage(string text, string nickname, string chatName)
        {
            string storageConnectionString = connectionString;
            //   !!! get from app settins or connection strings
            CloudStorageAccount storageAccount = null;

            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                string tableName = chatName;
                CloudTable cloudTable = tableClient.GetTableReference(tableName);
                await CreateNewTableAsync(cloudTable);

                MessageTable messageTable = new MessageTable();
                messageTable.Time = DateTime.UtcNow.ToLongTimeString();
                messageTable.AuthorNickName = nickname;
                messageTable.Body = text;

                messageTable.AssignPartitionKey();
                messageTable.AssignRowKey();


                TableOperation tableOperation = TableOperation.Insert(messageTable);
                await cloudTable.ExecuteAsync(tableOperation);
                return "Record inserted";
            }
            else
            {
                return "wrong connection string";
            }
        }

        public async Task<string> TableGetData(string chatName)
        {
            string storageConnectionString = connectionString;
            CloudStorageAccount storageAccount = null;

            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                string tableName = chatName;
                CloudTable cloudTable = tableClient.GetTableReference(tableName);
                await CreateNewTableAsync(cloudTable);

                return await DisplayTableRecordsAsync(cloudTable);
            }
            else
            {
                return "wrong connection string";
            }
        }

        public static async Task<string> DisplayTableRecordsAsync(CloudTable table)
        {
            TableQuery<MessageTable> tableQuery = new TableQuery<MessageTable>();
            TableContinuationToken token = null;
            string output = "";

            foreach (MessageTable message in await table.ExecuteQuerySegmentedAsync(tableQuery, token))
            {
                output += ("Time : " + message.Time + " || " + "Nickname : " + message.AuthorNickName + " || " + "Message : " + message.Body + Environment.NewLine);
            }
            return output;
        }

        public static async Task InsertRecordToTableAsync(CloudTable table, string time, string nickname, string value)
        {
            
            MessageTable message = new MessageTable();
            message.Time = time;
            message.AuthorNickName = nickname;
            message.Body = value;

            Message mess = await RetrieveRecordAsync(table, time, nickname);
            if (mess == null)
            {
                TableOperation tableOperation = TableOperation.Insert(message);
                await table.ExecuteAsync(tableOperation);
                //Console.WriteLine("Record inserted");
            }
            else
            {
                //Console.WriteLine("Record exists");
            }
        }

        public static async Task<Message> RetrieveRecordAsync(CloudTable table, string partitionKey, string rowKey)
        {
            TableOperation tableOperation = TableOperation.Retrieve<MessageTable>(partitionKey, rowKey);
            TableResult tableResult = await table.ExecuteAsync(tableOperation);
            return tableResult.Result as Message;
        }

        public static async Task<string> CreateNewTableAsync(CloudTable table)
        {
            if (!await table.CreateIfNotExistsAsync())
            {
                return "Table '" + table.Name + "' already exists";
            }
            return "Table '" + table.Name + "' created";
        }

    }
}
