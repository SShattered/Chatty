using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Chatty.Shared
{
    [Serializable]
    public class MessageBase
    {
        public Guid Id;
        public string _type { get; set; }

        public MessageBase() 
        {
            _type = GetType().Name;
        }
    }
}
