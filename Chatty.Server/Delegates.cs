using Chatty.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatty.Server
{
    public class Delegates
    {
        public delegate void ClientValidatingDelegate();
        public delegate void ClientBasicDelegate(Receiver receiver);
        public delegate void MessageReceived(object messageText);
    }
}
