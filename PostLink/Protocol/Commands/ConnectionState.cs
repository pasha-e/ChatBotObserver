using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerBotObserver.PostLink.Protocol.Commands
{
    public class ConnectionState
    {
        public string command => "connectionState";

        public string state { get; set; }

        public override string ToString()
        {
            return $"connectionState: {state}";
        }
    }
}
