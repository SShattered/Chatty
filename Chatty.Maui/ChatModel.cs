using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatty.Maui
{
    public class ChatModel
    {
        public enum User
        {
            Sender,
            Receiver
        }
        
        public string Message { get; set; }

        public User ChatUser { get; set; }
    }
}
