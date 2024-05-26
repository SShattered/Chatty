using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatty.Shared
{
    public class ResponseCallbackObject
    {
        public Delegate Callback { get; set; }
        public Guid Id { get; set; }
        public ResponseCallbackObject() { }
    }
}
