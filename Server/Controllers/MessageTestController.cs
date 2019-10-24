using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageTestController : ControllerBase
    {
        private IChatManager _chatManager;

        public MessageTestController(IChatManager chatManager)
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

            //MESSAGE
            /*
            Message message = new Message();
            message.time = DateTime.UtcNow.ToLongTimeString();
            message.authorNickName = "testNickname";
            message.body = "testMessage";

            _chatManager.StoreMessage("1", message);
            */
            //MESSAGE

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = text
            };
        }

        [HttpPost ("SendMessage")]
        public async Task PostAsync(string text, string nickname, string chatName, string guid)
        {
            Message message = new Message();
            message.time = DateTime.UtcNow.ToLongTimeString();
            message.authorNickName = nickname;
            message.body = text;

            CloudStorageAccount storageAccount = null;

            string storageConnectionString = ConfigurationManager.ConnectionStrings["AzureTableConStr"].ConnectionString;

            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                string tableName = "Chat";
                CloudTable cloudTable = tableClient.GetTableReference(tableName);
                await CreateNewTableAsync(cloudTable);

                //string time = "time"; //DateTime.UtcNow.ToLongTimeString()
                //string nickname = "Shrimp";
                //string value = "message"; //Guid.NewGuid().ToString();

                await InsertRecordToTableAsync(cloudTable, message.time, message.authorNickName, message.body); //ERROR -> look bellow
            }
            else
            {
                Console.WriteLine(
                    "Wrong connection string");
            }
        }

        public static async Task InsertRecordToTableAsync(CloudTable table, string time, string nickname, string value)
        {
            MessageTable messageTable = new MessageTable();
            messageTable.time = time;
            messageTable.authorNickName = nickname;
            messageTable.body = value;
            Message mess = await RetrieveRecordAsync(table, time, nickname);
            if (mess == null)
            {
                TableOperation tableOperation = TableOperation.Insert(messageTable);
                await table.ExecuteAsync(tableOperation); //ERROR : "bad request" ???
                Console.WriteLine("Record inserted");
            }
            else
            {
                Console.WriteLine("Record exists");
            }
        }
        public static async Task<Message> RetrieveRecordAsync(CloudTable table, string partitionKey, string rowKey)
        {
            TableOperation tableOperation = TableOperation.Retrieve<MessageTable>(partitionKey, rowKey);
            TableResult tableResult = await table.ExecuteAsync(tableOperation);
            return tableResult.Result as Message;
        }

        public static async Task CreateNewTableAsync(CloudTable table)
        {
            if (!await table.CreateIfNotExistsAsync())
            {
                Console.WriteLine("Table {0} already exists", table.Name);
                return;
            }
            Console.WriteLine("Table {0} created", table.Name);
        }
    }
}