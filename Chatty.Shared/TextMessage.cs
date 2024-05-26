using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Chatty.Shared
{
    [Serializable]
    public class TextMessage : MessageBase
    {
        public string Message { get; set; } 
        public TextMessage() : base() { }
    }
}