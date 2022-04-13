using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerBotObserver.PostLink.Protocol.Commands
{
    public class UserMessagesList
    {
        public string command => "messages";

        public string uuid { get; set; }

        public int total { get; set; }

        public int first { get; set; }

        public UserMessage[] list { get; set; }
    }
}
