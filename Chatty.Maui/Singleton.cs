using Chatty.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatty.Maui
{
    public sealed class Singleton
    {
        private readonly CClient _client;

        private Guid _clientGuid;
        public Guid ClientGuid { get; set; }

        Singleton()
        {
            _client = new CClient();
        }

        private static Singleton instance;
        public static Singleton Instance
        {
            get
            {
                instance ??= new Singleton();
                return instance;
            }
        }

        public CClient Client
        {
            get { return _client; } 
        }
    }
}
