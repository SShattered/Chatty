using Chatty.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatty.Client
{
    public class Delegates
    {
        public delegate void MessageReceived(TextMessage message);
        public delegate void ChatRequestReceived(Action<bool> callback, Guid refGuid);
        public delegate void ChatResponseReceived(bool status, Guid refGuid);
        public delegate void TextMessageReceived(TextMessage textMessage);
        public delegate void ChatDisconnected();
        public delegate void Connecting();
        public delegate void Disconnected();
        public delegate void Connected();
    }
}
