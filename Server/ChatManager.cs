using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

using Microsoft.WindowsAzure.Storage.Blob;
using System.Drawing;
using System.Diagnostics;

namespace Server
{

    public class ChatManager : IChatManager
    {
        private ConcurrentDictionary<string, List<Message>> _chats = new ConcurrentDictionary<string, List<Message>>();
        private ConcurrentDictionary<string, List<string>> _chatSessions = new ConcurrentDictionary<string, List<string>>();
        private ConcurrentDictionary<string, BufferBlock<Message>> _sessionListeners = new ConcurrentDictionary<string, BufferBlock<Message>>();
        private ConcurrentDictionary<string, BufferBlock<string>> _bufferBlocksChat = new ConcurrentDictionary<string, BufferBlock<string>>();

        private readonly string connectionString;
        private readonly string sas;
        private readonly string mlComVisStorageKey;

        private CloudStorageAccount storageAccount = null;
        CloudTableClient tableClient = null;
        CloudBlobClient blobClient = null;

        public ChatManager(string connectionString, string inputsas, string inputmlComVisStorageKey)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException("ConnectionString is null");
            this.sas = inputsas ?? throw new ArgumentNullException("Sas is null");
            this.mlComVisStorageKey = inputmlComVisStorageKey ?? throw new ArgumentNullException("ComVisKey is null");

            if (CloudStorageAccount.TryParse(connectionString, out storageAccount))
            {
                tableClient = storageAccount.CreateCloudTableClient();
                blobClient = storageAccount.CreateCloudBlobClient();
            }
            else
            {
                throw new ArgumentNullException("wrong connection string");
            }

            
        }


        public async Task activeUsersCleaner()
        {
                CloudTable cloudTable = tableClient.GetTableReference("activeUsers");

                TableContinuationToken token = null;
                var queryResult = cloudTable.ExecuteQuerySegmentedAsync(new TableQuery<ActiveUserTable>(), token);
                var entities = new List<ActiveUserTable>();
                entities.AddRange(queryResult.Result);

                foreach(var entity in entities)
                {
                    DateTime timeNow = DateTime.Now;
                    if (timeNow.Subtract(entity.TimeStart).TotalMinutes > 100)
                    {
                        TableOperation delete = TableOperation.Delete(entity);

                        await cloudTable.ExecuteAsync(delete);
                    }
                }
        }


        public async Task StoreMessageAsync(string chatName, Message message, string guid) //DOING WITH DB
        {
            if(!_chats.ContainsKey(chatName))
            {
                _chats[chatName] = new List<Message>(); 
            }
            _chats[chatName].Add(message);

            if(message.isPicture == false)
            {
                //DB
                await DBStoreMessage(message.body, message.authorNickName, chatName);
                //DB
            }

            if (_chatSessions.TryGetValue(chatName, out var sessionIds))
            {
                foreach (var sessionId in sessionIds)
                {
                    await _sessionListeners[sessionId].SendAsync(message);
                }
            }
        }
        
        public NewSessionResult GetLastMessages(string chatName, int count) //DOING WITH DB
        {
            string guid = Guid.NewGuid().ToString();
            _sessionListeners[guid] = new BufferBlock<Message>();
            if (!_chatSessions.ContainsKey(chatName))
            {
                _chatSessions[chatName] = new List<string>();
            }
            _chatSessions[chatName].Add(guid);

            List<Message> outputMessages = new List<Message>();
            List<Message> output = new List<Message>();


            try
            {
                var table = tableClient.GetTableReference(chatName);
                TableContinuationToken token = null;
                var queryResult = table.ExecuteQuerySegmentedAsync(new TableQuery<MessageTable>(), token);
                var entities = new List<MessageTable>();
                entities.AddRange(queryResult.Result);
                
                for (int i = 0; i < entities.Count; i++)
                {
                    MessageTable messageTable = entities[i];
                    Message message = new Message();

                    message.body = messageTable.Body;
                    message.time = messageTable.Time;
                    message.authorNickName = messageTable.AuthorNickName;
                    output.Add(message);
                }
                

                if(output.Count <= count)
                {
                    outputMessages = output;
                }
                else
                {
                    for(int i=output.Count-count-1; i<output.Count; i++)
                    {
                        outputMessages.Add(output[i]);
                    }
                }
                
            }
            catch
            {
                
            }
            return new NewSessionResult { sessionId = guid, lastMessages = outputMessages, sas = sas};
        }

        public string GetSasTest()
        {
            return sas;
        }

        public string GetVisKeyTest()
        {
            return mlComVisStorageKey;
        }



        public async Task<NewSessionResultChats> GetChatsSessionAsync(int count)
        {
            string guid = Guid.NewGuid().ToString();
            _bufferBlocksChat[guid] = new BufferBlock<string>();

            List<string> chats = new List<string>();

            List<string> output = new List<string>();
            TableContinuationToken token = null;

            var tables = await tableClient.ListTablesSegmentedAsync(token);

            foreach (var table in tables)
            {
                if (table.Name != "users" && table.Name != "activeUsers") output.Add(table.Name);
            }

            return new NewSessionResultChats { sessionId = guid, chats = output };
        }

        public async Task<Message> GetNewMessageAsync(string chatName, string sessionId)
        {
            return await _sessionListeners[sessionId].ReceiveAsync();
        }

        public async Task<string> GetNewChatAsync(string sessionId)
        {
            return await _bufferBlocksChat[sessionId].ReceiveAsync();
        }

        public async Task<List<string>> GetChatsAsync()
        {
            List<string> output = new List<string>();
            TableContinuationToken token = null;

            var tables = await tableClient.ListTablesSegmentedAsync(token);

            foreach (var table in tables)
            {
                if(table.Name != "users") output.Add(table.Name);
            }
            return output;
        }

        public async void CreateChat(string chatName)
        {

            CloudTable cloudTable = this.tableClient.GetTableReference(chatName);
            await CreateNewTableAsync(cloudTable);

            CloudBlobContainer container = this.blobClient.GetContainerReference(chatName);
            await CreateNewBlobContainerAsync(container);


            if (!_chats.ContainsKey(chatName)) // look if there is this chat in DB 
            {
                _chats[chatName] = new List<Message>();

                foreach (var bufferBlock in _bufferBlocksChat)
                {
                    await bufferBlock.Value.SendAsync(chatName);
                }
            }
        }

        public async Task<List<string>> DBGetAllChats()
        {
            List<string> output = new List<string>();
            TableContinuationToken token = null;

            var tables = await tableClient.ListTablesSegmentedAsync(token);

            foreach (var table in tables)
            {
                output.Add(table.Name);
            }

            return output;
        }

        public void DeleteChats()
        {
            _chats.Clear();
        }

        public async Task DeleteChatAsync(string chatName) //DOING WITH DB
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

                CloudBlobClient client = storageAccount.CreateCloudBlobClient();

                CloudBlobContainer container = client.GetContainerReference(chatName); 

                if (await cloudTable.DeleteIfExistsAsync() && await container.DeleteIfExistsAsync()) return "deleted";
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

            CloudTable cloudTable = tableClient.GetTableReference(chatName);
            //await CreateNewTableAsync(cloudTable);

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

        public async Task<string> TableGetData(string chatName)
        {
            string storageConnectionString = connectionString;
            CloudStorageAccount storageAccount = null;

            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                string tableName = chatName;
                CloudTable cloudTable = tableClient.GetTableReference(tableName);

                return await DisplayTableRecordsAsync(cloudTable);
            }
            else
            {
                return "wrong connection string";
            }
        }
        //DB TESTING 
        public List<Message> DBGetChatsMessages(string chatName)
        {
            var table = tableClient.GetTableReference(chatName);
            TableContinuationToken token = null;
            var queryResult = table.ExecuteQuerySegmentedAsync(new TableQuery<MessageTable>(), token);
            var entities = new List<MessageTable>();
            entities.AddRange(queryResult.Result);

            List<Message> output = new List<Message>();
            Message message = new Message();

            foreach (var entity in entities)
            {
                message.body = entity.Body;
                message.time = entity.Time;
                message.authorNickName = entity.AuthorNickName;
                output.Add(message);
            }
            return output;
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
        public static async Task<string> CreateNewBlobContainerAsync(CloudBlobContainer container)
        {
            if (!await container.CreateIfNotExistsAsync())
            {
                return "Table '" + container.Name + "' already exists";
            }
            return "Table '" + container.Name + "' created";
        }

        public async Task<string> CreateNewBlobContainerTestAsync(string chatName)
        {

            CloudBlobContainer container = this.blobClient.GetContainerReference(chatName);

            if (!await container.CreateIfNotExistsAsync())
            {
                return "Table '" + container.Name + "' already exists";
            }
            return "Table '" + container.Name + "' created";
        }



        //AUTHENTIFICATION

        //ADD NEW USER IN DB
        public async Task<string> DBStoreUser(string login, string hash, string nickname, string level)
        {

            CloudTable cloudTable = tableClient.GetTableReference("users");
            //await CreateNewTableAsync(cloudTable);

            UserTable userTable = new UserTable();
            userTable.Login = login;
            userTable.Hash = hash;
            userTable.Nickname = nickname;
            userTable.Level = level;
            userTable.Photo = "";

            userTable.AssignPartitionKey();
            userTable.AssignRowKey();


            TableOperation tableOperation = TableOperation.Insert(userTable);
            await cloudTable.ExecuteAsync(tableOperation);
            return "Record inserted";
        }
        public async Task<bool> CheckCredentialsAsync(string login, string hash)
        {
            try {
                string partitionKey = hash;
                string rowKey = login;
                var table = tableClient.GetTableReference("users");
                TableOperation retrieve = TableOperation.Retrieve<UserTable>(partitionKey, rowKey);

                TableResult result = await table.ExecuteAsync(retrieve);

                if(result.Result != null) return true;
                else return false;


            }
            catch(Exception e)
            {
                return false;
            }
        }

        public async Task<ResultSignIn> GetResultLoginAsync(string login, string password)
        {

            //check login and pass -> return null or userID
            try
            {
                string partitionKey = password;
                string rowKey = login;
                var table = tableClient.GetTableReference("users");
                TableOperation retrieve = TableOperation.Retrieve<UserTable>(partitionKey, rowKey);

                TableResult result = await table.ExecuteAsync(retrieve);

                if (result.Result != null) {

                    string userId = Guid.NewGuid().ToString();

                    ResultSignIn output = new ResultSignIn();
                    output.userID = userId;


                    CloudTable cloudTable = tableClient.GetTableReference("activeUsers");
                    //await CreateNewTableAsync(cloudTable);

                    ActiveUserTable userTable = new ActiveUserTable();
                    userTable.Login = login;
                    userTable.UserID = userId;
                    userTable.TimeStart = DateTime.Now;

                    userTable.AssignPartitionKey();
                    userTable.AssignRowKey();


                    TableOperation tableOperation = TableOperation.Insert(userTable);
                    await cloudTable.ExecuteAsync(tableOperation);

                    await activeUsersCleaner();

                    return output;
                }
                else return null;


            }
            catch (Exception e)
            {
                return null;
            }
            
        }

        public async Task<bool> CheckActiveUserIDsAsync(string activeUserID, string login)
        {
            try
            {
                string partitionKey = activeUserID;
                string rowKey = login;
                var table = tableClient.GetTableReference("activeUsers");
                TableOperation retrieve = TableOperation.Retrieve<ActiveUserTable>(partitionKey, rowKey);

                TableResult result = await table.ExecuteAsync(retrieve);

                if (result.Result != null)
                {


                    return true;
                }
                else return false;


            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<string> ChangeUserPicture(string login, string hash, string pictureName)
        {
            try
            {
                var table = tableClient.GetTableReference("users");

                string partitionKey = hash;
                string rowKey = login;

                TableOperation retrieve = TableOperation.Retrieve<UserTable>(partitionKey, rowKey);

                Trace.TraceInformation($"getting data for {partitionKey}:{rowKey}");

                TableResult result = await table.ExecuteAsync(retrieve);

                Trace.TraceInformation($"result:  {result?.Result?.GetType()?.ToString()}");

                UserTable thanks = (UserTable)result.Result;

                thanks.ETag = "*";
                thanks.Photo = pictureName;

                if (result != null)
                {
                    TableOperation update = TableOperation.Replace(thanks);

                    await table.ExecuteAsync(update);
                }
                return "DONE";
            }
            catch(Exception e)
            {
                return e.ToString() + " || " + login + " || " + hash;
            }
            
        }
        public async Task<string> ChangeUserPictureNew(string login, string pictureName)
        {
            try
            {
                var table = tableClient.GetTableReference("users");

                
                string rowKey = login;

                UserTable thanks = await GetUserByLogin(login);

                thanks.ETag = "*";
                thanks.Photo = pictureName;

                if (thanks != null)
                {
                    TableOperation update = TableOperation.Replace(thanks);

                    await table.ExecuteAsync(update);
                }
                return "DONE";
            }
            catch (Exception e)
            {
                return e.ToString();
            }

        }

        public async Task<string> ChangeUserNickname(string login, string nickname)
        {
            try
            {
                var table = tableClient.GetTableReference("users");


                string rowKey = login;

                UserTable thanks = await GetUserByLogin(login);

                thanks.ETag = "*";
                thanks.Nickname = nickname;

                if (thanks != null)
                {
                    TableOperation update = TableOperation.Replace(thanks);

                    await table.ExecuteAsync(update);
                }
                return "DONE";
            }
            catch (Exception e)
            {
                return e.ToString();
            }

        }
        public async Task<UserTable> GetUserByLogin(string login)
        {
            try
            {
                var table = tableClient.GetTableReference("users");

                string rowKey = login;

                TableQuery<UserTable> rangeQuery = new TableQuery<UserTable>().Where(
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey));

                TableContinuationToken token = null;
                foreach (UserTable entity in await table.ExecuteQuerySegmentedAsync(rangeQuery, token))
                {
                    return entity;
                }
                return null;
            }
            catch (Exception e)
            {
                //return e.ToString();
                return null;
            }

        }

        public async Task<string> ChangeUserPictureTest(string login, string hash, string pictureName)
        {
            try
            {
                var table = tableClient.GetTableReference("users");

                string partitionKey = hash;
                string rowKey = login;

                
                TableOperation retrieve = TableOperation.Retrieve<UserTable>(partitionKey, rowKey);

                //Trace.TraceInformation($"getting data for {partitionKey}:{rowKey}");

                TableResult result = await table.ExecuteAsync(retrieve);

                UserTable oldUser = (UserTable)result.Result;

                //return oldUser.Login.ToString();

                TableBatchOperation batchDeleteOperation = new TableBatchOperation();



                batchDeleteOperation.Delete((UserTable)result.Result);

                // Execute the batch operation.
                await table.ExecuteBatchAsync(batchDeleteOperation);

                
                UserTable userTable = new UserTable();
                userTable.Login = login;
                userTable.Hash = hash;
                userTable.Nickname = oldUser.Nickname;
                userTable.Level = oldUser.Level;
                userTable.Photo = pictureName;

                userTable.AssignPartitionKey();
                userTable.AssignRowKey();


                TableOperation tableOperation = TableOperation.Insert(userTable);
                await table.ExecuteAsync(tableOperation);

                

                return "DONE";
            }
            catch (Exception e)
            {
                return e.ToString() + " || " + login + " || " + hash;
            }

        }



        //GET USERS TABLE
        public List<UserTable> GetUserTableTest(string tableName)
        {
            var entities = new List<UserTable>();
            try
            {
                var table = tableClient.GetTableReference(tableName);
                TableContinuationToken token = null;
                var queryResult = table.ExecuteQuerySegmentedAsync(new TableQuery<UserTable>(), token);

                entities.AddRange(queryResult.Result);
            }
            catch
            {

            }
            return entities;
        }
        public async Task<string> DeleteAllUsersTestAsync()
        {
            try
            {
                CloudTable table = tableClient.GetTableReference("users");
                var query = new TableQuery<UserTable>();
                var result = await table.ExecuteQuerySegmentedAsync(query, null);

                // Create the batch operation.
                TableBatchOperation batchDeleteOperation = new TableBatchOperation();

                foreach (var row in result)
                {
                    batchDeleteOperation.Delete(row);
                }

                // Execute the batch operation.
                await table.ExecuteBatchAsync(batchDeleteOperation);
                return "deleted";
            }
            catch(Exception e)
            {
                return e.ToString();
            }
        }




        //FOR COMPRESSING IMAGES TO COMPARE THEM -- IN MLimages solution
        public static List<bool> GetHash(Bitmap bmpSource)
        {
            List<bool> lResult = new List<bool>();
            //create new image with 16x16 pixel
            Bitmap bmpMin = new Bitmap(bmpSource, new Size(16, 16));
            for (int j = 0; j < bmpMin.Height; j++)
            {
                for (int i = 0; i < bmpMin.Width; i++)
                {
                    //reduce colors to true / false                
                    lResult.Add(bmpMin.GetPixel(i, j).GetBrightness() < 0.5f);
                }
            }
            return lResult;
        }




        //TESTTING 
        public Message GetLastMessageTest(string chatName, int count)
        {
            Message message = new Message();
            try
            {
                var table = tableClient.GetTableReference(chatName);
                TableContinuationToken token = null;
                var queryResult = table.ExecuteQuerySegmentedAsync(new TableQuery<MessageTable>(), token);
                var entities = new List<MessageTable>();
                entities.AddRange(queryResult.Result);


                MessageTable messageTable = entities[count];

                message.body = messageTable.Body;
                message.time = messageTable.Time;
                message.authorNickName = messageTable.AuthorNickName;
            }
            catch
            {

            }
            return message;
        }

        public List<MessageTable> GetMessageTableTest(string chatName)
        {
            var entities = new List<MessageTable>();
            try
            {
                var table = tableClient.GetTableReference(chatName);
                TableContinuationToken token = null;
                var queryResult = table.ExecuteQuerySegmentedAsync(new TableQuery<MessageTable>(), token);
                
                entities.AddRange(queryResult.Result);
            }
            catch
            {

            }
            return entities;
        }



        //SIFROVACKA

        class Sifrovacka_user : TableEntity
        {

            private string name;
            private string surname;
            private int stage;

            public void AssignRowKey()
            {
                this.RowKey = name;
            }
            public void AssignPartitionKey()
            {
                this.PartitionKey = surname;
            }
            public string Name
            {
                get
                {
                    return name;
                }

                set
                {
                    name = value;
                }
            }
            public string Surname
            {
                get
                {
                    return surname;
                }

                set
                {
                    surname = value;
                }
            }
            public int Stage
            {
                get
                {
                    return stage;
                }

                set
                {
                    stage = value;
                }
            }

        }


        class Sifrovacka_stage : TableEntity
        {

            private string Stage;
            private string Position;
            private string Name;
            private string Popis;

            public void AssignRowKey()
            {
                this.RowKey = Stage;
            }
            public void AssignPartitionKey()
            {
                this.PartitionKey = Stage;
            }
            public string stage
            {
                get
                {
                    return Stage;
                }

                set
                {
                    Stage = value;
                }
            }
            public string position
            {
                get
                {
                    return Position;
                }

                set
                {
                    Position = value;
                }
            }
            public string name
            {
                get
                {
                    return Name;
                }

                set
                {
                    Name = value;
                }
            }
            public string popis
            {
                get
                {
                    return Popis;
                }

                set
                {
                    Popis = value;
                }
            }
        }

        class Sifrovacka_zadani : TableEntity
        {

            private string Stage;
            private string Zadani;
            private string ZadaniMensi;
            private string Reseni1;
            private string Reseni2;
            private string Napoveda1;
            private string Napoveda2;
            private string Napoveda3;

            public void AssignRowKey()
            {
                this.RowKey = Stage;
            }
            public void AssignPartitionKey()
            {
                this.PartitionKey = Stage;
            }
            public string stage
            {
                get
                {
                    return Stage;
                }

                set
                {
                    Stage = value;
                }
            }
            public string zadani
            {
                get
                {
                    return Zadani;
                }

                set
                {
                    Zadani = value;
                }
            }

            public string zadaniMensi
            {
                get
                {
                    return ZadaniMensi;
                }

                set
                {
                    ZadaniMensi = value;
                }
            }

            public string reseni1
            {
                get
                {
                    return Reseni1;
                }

                set
                {
                    Reseni1 = value;
                }
            }

            public string reseni2
            {
                get
                {
                    return Reseni2;
                }

                set
                {
                    Reseni2 = value;
                }
            }

            public string napoveda1
            {
                get
                {
                    return Napoveda1;
                }

                set
                {
                    Napoveda1 = value;
                }
            }
            public string napoveda2
            {
                get
                {
                    return Napoveda2;
                }

                set
                {
                    Napoveda2 = value;
                }
            }
            public string napoveda3
            {
                get
                {
                    return Napoveda3;
                }

                set
                {
                    Napoveda3 = value;
                }
            }
        }



        public async Task<bool> SifrovackaGetResultLoginAsync(string name, string surname)
        {

            try
            {
                var table = tableClient.GetTableReference("sifrovacka");

                string partitionKey = name;
                string rowKey = surname;


                TableOperation retrieve = TableOperation.Retrieve<UserTable>(partitionKey, rowKey);

                TableResult result = await table.ExecuteAsync(retrieve);

                if (result.Result != null)
                {
                    Sifrovacka_user user = result.Result as Sifrovacka_user;

                    return true;

                }
                else
                {

                    Sifrovacka_user user = new Sifrovacka_user();

                    user.Name = name;
                    user.Surname = surname;
                    user.Stage = 1;
                    user.AssignPartitionKey();
                    user.AssignRowKey();

                    TableOperation tableOperation = TableOperation.Insert(user);
                    await table.ExecuteAsync(tableOperation);

                    return true;

                }
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public async Task<string> SifrovackaNavigaceGetLocation(string name, string surname)
        {

            try
            {
                

                var table = tableClient.GetTableReference("sifrovacka");

                string partitionKey = surname;
                string rowKey = name;


                //if (partitionKey == name) return "partitionKey == name";
                //else if(rowKey == surname) return "rowKey == surname";
                //else return "false";

                TableQuery<Sifrovacka_user> rangeQuery = new TableQuery<Sifrovacka_user>().Where(
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey)
                        )
                    );

                /*
                TableQuery<Sifrovacka_user> rangeQuery = new TableQuery<Sifrovacka_user>().Where( 
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey));
                */

                TableContinuationToken token = null;

                foreach (Sifrovacka_user user in await table.ExecuteQuerySegmentedAsync(rangeQuery, token))
                {
                    //return rowKey;
                    //return user.Name;

                    string partitionKeyStages = user.Stage.ToString();
                    string rowKeyStages = user.Stage.ToString();

                    var tableStages = tableClient.GetTableReference("sifrovackaStages");

                    TableQuery<Sifrovacka_stage> rangeQueryStages = new TableQuery<Sifrovacka_stage>().Where(
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKeyStages));

                   

                    TableContinuationToken tokenStages = null;
                    foreach (Sifrovacka_stage stage in await tableStages.ExecuteQuerySegmentedAsync(rangeQueryStages, tokenStages))
                    {
                        //return rowKeyStages;
                        //return user.Stage.ToString();
                        return stage.position;
                        //return 3.ToString();
                    }
                }
                return "user not found";
            }
            catch (Exception e)
            {
                return e.ToString();
            }

        }

        public async Task<Zadani> SifrovackaNavigaceGetZadani(string name, string surname)
        {

            try
            {


                var table = tableClient.GetTableReference("sifrovacka");

                string partitionKey = surname;
                string rowKey = name;

                TableQuery<Sifrovacka_user> rangeQuery = new TableQuery<Sifrovacka_user>().Where(
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey)
                        )
                    );

                TableContinuationToken token = null;
                foreach (Sifrovacka_user user in await table.ExecuteQuerySegmentedAsync(rangeQuery, token))
                {
                    string partitionKeyStages = user.Stage.ToString();
                    string rowKeyStages = user.Stage.ToString();

                    var tableStages = tableClient.GetTableReference("sifrovackaZadani");

                    TableQuery<Sifrovacka_zadani> rangeQueryStages = new TableQuery<Sifrovacka_zadani>().Where(
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKeyStages));



                    TableContinuationToken tokenStages = null;
                    foreach (Sifrovacka_zadani stage in await tableStages.ExecuteQuerySegmentedAsync(rangeQueryStages, tokenStages))
                    {
                        //return user.Stage.ToString();
                        Zadani zadani = new Zadani();

                        //return stage.napoveda1;

                        zadani.zadani = stage.zadani;
                        zadani.zadaniMensi = stage.zadaniMensi;
                        zadani.reseni1 = stage.reseni1;
                        zadani.reseni2 = stage.reseni2;
                        zadani.napoveda1 = stage.napoveda1;
                        zadani.napoveda2 = stage.napoveda2;
                        zadani.napoveda3 = stage.napoveda3;

                        return zadani;

                    }
                }
                return null;
            }
            catch (Exception e)
            {
                return null;
            }

        }

        public async Task<string> SifrovackaNavigaceGetSolution(string name, string surname)
        {

            try
            {


                var table = tableClient.GetTableReference("sifrovacka");

                string partitionKey = surname;
                string rowKey = name;

                TableQuery<Sifrovacka_user> rangeQuery = new TableQuery<Sifrovacka_user>().Where(
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey)
                        )
                    );

                TableContinuationToken token = null;
                foreach (Sifrovacka_user user in await table.ExecuteQuerySegmentedAsync(rangeQuery, token))
                {
                    string partitionKeyStages = user.Stage.ToString();
                    string rowKeyStages = user.Stage.ToString();

                    var tableStages = tableClient.GetTableReference("sifrovackaZadani");

                    TableQuery<Sifrovacka_zadani> rangeQueryStages = new TableQuery<Sifrovacka_zadani>().Where(
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKeyStages));



                    TableContinuationToken tokenStages = null;
                    foreach (Sifrovacka_zadani stage in await tableStages.ExecuteQuerySegmentedAsync(rangeQueryStages, tokenStages))
                    {
                        //return user.Stage.ToString();
                        return stage.zadani;

                    }
                }
                return null;
            }
            catch (Exception e)
            {
                return null;
            }

        }

        public async Task<string> SifrovackaSifraSubmit(string name, string surname)
        {
            try
            {
                var table = tableClient.GetTableReference("sifrovacka");

                string partitionKey = surname;
                string rowKey = name;

                TableQuery<Sifrovacka_user> rangeQuery = new TableQuery<Sifrovacka_user>().Where(
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey)
                        )
                    );

                TableContinuationToken token = null;
                foreach (Sifrovacka_user user in await table.ExecuteQuerySegmentedAsync(rangeQuery, token))
                {

                    if(user.Stage == 1)
                    {
                        //Random rnd = new Random();
                        //user.Stage += rnd.Next(1, 3);
                        user.Stage += 1;
                    }
                    else
                    {
                        user.Stage += 1;
                    }

                    TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(user);

                    TableResult result = await table.ExecuteAsync(insertOrReplaceOperation);

                    return result.ToString();
                }
                return "didnt find user";
            }
            catch (Exception e)
            {
                return e.ToString();
            }

        }


        public async Task<Misto> SifrovackaMistoGet(string name, string surname)
        {
            try
            {

                var table = tableClient.GetTableReference("sifrovacka");

                string partitionKey = surname;
                string rowKey = name;

                TableQuery<Sifrovacka_user> rangeQuery = new TableQuery<Sifrovacka_user>().Where(
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey)
                        )
                    );

                TableContinuationToken token = null;
                foreach (Sifrovacka_user user in await table.ExecuteQuerySegmentedAsync(rangeQuery, token))
                {
                    string partitionKeyStages = user.Stage.ToString();
                    string rowKeyStages = user.Stage.ToString();

                    var tableStages = tableClient.GetTableReference("sifrovackaStages");

                    TableQuery<Sifrovacka_stage> rangeQueryStages = new TableQuery<Sifrovacka_stage>().Where(
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKeyStages));



                    TableContinuationToken tokenStages = null;
                    foreach (Sifrovacka_stage stage in await tableStages.ExecuteQuerySegmentedAsync(rangeQueryStages, tokenStages))
                    {
                        //return user.Stage.ToString();
                        Misto misto = new Misto();

                        //return stage.napoveda1;

                        misto.nazev = stage.name;
                        misto.popis = stage.popis;

                        return misto;

                    }
                }

                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public async Task<Vitezove> SifrovackaVitezoveGet()
        {
            try
            {

                var table = tableClient.GetTableReference("sifrovacka");


                TableQuery<Sifrovacka_user> rangeQuery = new TableQuery<Sifrovacka_user>().Where(
                            TableQuery.GenerateFilterConditionForInt("Stage", QueryComparisons.Equal, 8)
                    );
                
                Vitezove vitezove = new Vitezove();
                List<String> jmena = new List<string>();
                List<String> prijmeni = new List<string>();
                int index = 0;
                TableContinuationToken token = null;
                TableQuerySegment<Sifrovacka_user> result = await table.ExecuteQuerySegmentedAsync(rangeQuery, token);
                foreach (Sifrovacka_user user in result)
                {
                    jmena.Add(user.Name);
                    prijmeni.Add(user.Surname);

                    index++;
                }

                String[] str = jmena.ToArray();
                String[] str2 = prijmeni.ToArray();

                vitezove.seznam_jmen = str;
                vitezove.seznam_prijmeni = str2;

                return vitezove;
                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<string> Sifrovacka_Test()
        {
            try
            {

                var table = tableClient.GetTableReference("sifrovacka");

                string partitionKey = "";
                string rowKey = "";

                TableQuery<Sifrovacka_user> rangeQuery = new TableQuery<Sifrovacka_user>().Where(
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey)
                        )
                    );

                TableContinuationToken token = null;
                foreach (Sifrovacka_user user in await table.ExecuteQuerySegmentedAsync(rangeQuery, token))
                {
                    return user.Name;
                }

                    return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
