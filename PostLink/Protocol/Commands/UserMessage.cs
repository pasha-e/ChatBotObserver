using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerBotObserver.PostLink.Protocol.Commands
{
    public class UserMessage
    {
        public long id { get; set; }

        public long targetId { get; set; }

        public long ownerId { get; set; }

        public string senderName { get; set; }

        public string senderType { get; set; }

        public long time { get; set; }

        public string title { get; set; }

        public string message { get; set; }

        public bool crypted { get; set; }

        public long receivedTime { get; set; }
        public long readedTime { get; set; }

        public bool unreadable { get; set; }

        public bool autoread { get; set; }
        public FilesDescription[] files { get; set; }

        public int fileTargetType { get; set; }

        public long fileTargetId { get; set; }
        public UidAttributes attributes { get; set; }
    }
}
