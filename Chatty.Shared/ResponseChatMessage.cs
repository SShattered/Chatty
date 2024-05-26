using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatty.Shared
{
    [Serializable]
    public class ResponseChatMessage : ResponseMessage
    {
        public bool Accepted { get; set; }
        public int NumOfClients { get; set; }
        public Guid ClientGuid { get; set; }
        public ResponseChatMessage() { }
    }
}
