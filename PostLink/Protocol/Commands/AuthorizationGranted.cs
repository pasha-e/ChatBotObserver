using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerBotObserver.PostLink.Protocol.Commands
{
    public class AuthorizationGranted
    {
        public string command { get; set; }

        public int userId { get; set; }

        public long timeZoneOffset { get; set; }

        public long deltaTime { get; set; }

        public int[] contacts { get; set; }
    }
}
