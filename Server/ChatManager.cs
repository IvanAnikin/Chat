using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Server
{

    public class ChatManager : IChatManager
    {
        private ConcurrentDictionary<string, List<Message>> _chats = new ConcurrentDictionary<string, List<Message>>();
        private ConcurrentDictionary<string, BufferBlock<Message>> _bufferBlocks = new ConcurrentDictionary<string, BufferBlock<Message>>();
        private ConcurrentDictionary<string, BufferBlock<string>> _bufferBlocksChat = new ConcurrentDictionary<string, BufferBlock<string>>();
        //private BufferBlock<Message> _messageQueue = new BufferBlock<Message>();

        public void StoreMessage(string chatName, Message message)
        {
            if(!_chats.ContainsKey(chatName))
            {
                _chats[chatName] = new List<Message>(); 
            }
            _chats[chatName].Add(message);

            foreach(var bufferBlock in _bufferBlocks)
            {
                bufferBlock.Value.SendAsync(message);
            }
        }
        
        public NewSessionResult GetLastMessages(string chatName, int count)
        {
            string guid = Guid.NewGuid().ToString();
            _bufferBlocks[guid] = new BufferBlock<Message>();

            var messages = new List<Message>();
            List<Message> outputMessages = new List<Message> { };

            try
            {
                messages = _chats[chatName];

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
            return await _bufferBlocks[sessionId].ReceiveAsync();
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
            _bufferBlocks.TryRemove(sessionId, out BufferBlock<Message> value);
        }
    }
}
