using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerBotObserver.PostLink.Protocol.Commands
{
    public class SendMessageResult
    {
        public string command => "sendMessageResult";

        public string uuid { get; set; }

        public UserMessage result { get; set; }
    }
}
