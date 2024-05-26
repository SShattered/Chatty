using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatty.Shared
{
    [Serializable]
    public class RequestChatMessage : RequestMessage
    {
        public Guid ClientGuid { get; set; }
        public RequestChatMessage() { }
    }
}
