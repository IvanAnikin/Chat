using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    }    
}
