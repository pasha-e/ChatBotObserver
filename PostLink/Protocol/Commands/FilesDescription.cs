using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerBotObserver.PostLink.Protocol.Commands
{
    public class FilesDescription
    {
        public long fileId { get; set; }

        public string name { get; set; }

        public int size { get; set; }

        public bool dir { get; set; }

        public long ctime { get; set; }

        public int state { get; set; }
    }
}
