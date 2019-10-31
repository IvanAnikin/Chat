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

        public void StoreMessage(string chatName, Message message, string guid)
        {
            if(!_chats.ContainsKey(chatName))
            {
                _chats[chatName] = new List<Message>(); 
            }
            _chats[chatName].Add(message);
            //SendToTableAsync(message.body, message.authorNickName, chatName, connStr);

            if (_chatSessions.TryGetValue(chatName, out var sessionIds))
            {
                foreach (var sessionId in sessionIds)
                {
                    _sessionListeners[sessionId].SendAsync(message);
                }
            }
        }
        
        public NewSessionResult GetLastMessages(string chatName, int count)
        {
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

        public void CreateChat(string chatName)
        {
            if (!_chats.ContainsKey(chatName))
            {
                _chats[chatName] = new List<Message>();

                foreach (var bufferBlock in _bufferBlocksChat)
                {
                    bufferBlock.Value.SendAsync(chatName);
                }
            }
        }

        public void DeleteChats()
        {
            _chats.Clear();
        }

        public void DeleteChat(string chatName)
        {
            _chats.Remove(chatName, out List<Message> value);
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

        public async Task<string> SendToTableTestAsync(string text, string nickname, string chatName, string connStr)
        {
            //string storageConnectionString = connStr;
            string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=notesaccount;AccountKey=3/h/oSu1aRCzPOyUXy9YqOCDHTVGJJKpiM4NkFcbEBDHf38gKB1XGP8NqbcGLtj3e2rud2jBqe7seF3giFziow==;EndpointSuffix=core.windows.net";
            CloudStorageAccount storageAccount = null;

            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                string tableName = "chats";
                CloudTable cloudTable = tableClient.GetTableReference(tableName);
                await CreateNewTableAsync(cloudTable);

                /*
                MessageTableTest messageTable = new MessageTableTest();
                messageTable.Body = text;
                messageTable.Time = DateTime.UtcNow.ToLongTimeString();

                messageTable.AssignPartitionKey();
                messageTable.AssignRowKey();
                */

                MessageTable messageTable = new MessageTable();
                messageTable.ChatName = chatName;
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



        public async Task<string> SendToTableAsync(string text, string nickname, string chatName, string connStr)
        {
            //string storageConnectionString = ConfigurationManager.AppSettings["tablestoragecs"];
            //string storageConnectionString = connStr; //STORAGE CONNECTION STRING !!!
            string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=notesaccount;AccountKey=3/h/oSu1aRCzPOyUXy9YqOCDHTVGJJKpiM4NkFcbEBDHf38gKB1XGP8NqbcGLtj3e2rud2jBqe7seF3giFziow==;EndpointSuffix=core.windows.net";
            CloudStorageAccount storageAccount = null;

            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                string tableName = "chats";
                CloudTable cloudTable = tableClient.GetTableReference(tableName);
                await CreateNewTableAsync(cloudTable);

                return await InsertRecordToTableStrAsync(cloudTable, DateTime.UtcNow.ToLongTimeString(), nickname, text, chatName);

                //return "connection string approved :-)";
            }
            else
            {
                return "wrong connection string";
            }
        }

        public async Task<string> TableGetData()
        {
            string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=notesaccount;AccountKey=3/h/oSu1aRCzPOyUXy9YqOCDHTVGJJKpiM4NkFcbEBDHf38gKB1XGP8NqbcGLtj3e2rud2jBqe7seF3giFziow==;EndpointSuffix=core.windows.net";
            CloudStorageAccount storageAccount = null;

            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                string tableName = "chats";
                CloudTable cloudTable = tableClient.GetTableReference(tableName);
                await CreateNewTableAsync(cloudTable);

                return await DisplayTableRecordsAsync(cloudTable);

                //return "connection string approved :-)";
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
                /*
                Console.WriteLine("Time : {0}", message.Time);
                Console.WriteLine("NIckname : {0}", message.AuthorNickName);
                Console.WriteLine("Message : {0}", message.Body);
                Console.WriteLine("******************************");
                */
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
        public static async Task<string> InsertRecordToTableStrAsync(CloudTable table, string time, string nickname, string value, string chatName)
        {
            /*
            MessageTable message = new MessageTable();
            message.Time = time;
            message.AuthorNickName = nickname;
            message.Body = value;

            message.AssignPartitionKey();
            message.AssignRowKey();
            */

            MessageTableNew messageTable = new MessageTableNew();
            messageTable.ChatName = chatName;
            messageTable.Message[0] = time;
            messageTable.Message[1] = nickname;
            messageTable.Message[2] = value;

            messageTable.AssignPartitionKey();
            messageTable.AssignRowKey();

            TableOperation tableOperation = TableOperation.Insert(messageTable);
            await table.ExecuteAsync(tableOperation);
            return "Record inserted";

            /*
            Message mess = await RetrieveRecordAsync(table, time, nickname);
            if (mess == null)
            {
                TableOperation tableOperation = TableOperation.Insert(messageTable);
                await table.ExecuteAsync(tableOperation);
                return "Record inserted";
            }
            else
            {
                return "Record exists";
            }
            */
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
                //Console.WriteLine("Table {0} already exists", table.Name);
                return "Table '" + table.Name + "' already exists";
            }
            //Console.WriteLine("Table {0} created", table.Name);
            return "Table '" + table.Name + "' created";
        }

    }
}
