using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessengerBotObserver.PostLink.Protocol.Commands.Attachments;

namespace MessengerBotObserver.PostLink.Protocol.Commands
{
    public class SendMessage
    {
        public string command => "sendMessage";

        public string receiverId { get; set; }

        //public string uuid { get; set; }

        public string receiverType { get; set; }

        public bool unreadable { get; set; }

        public bool autoread { get; set; }

        public string message { get; set; }

        public Attachment[] files { get; set; }

        public UidAttributes attributes { get; set; }
    }
}
