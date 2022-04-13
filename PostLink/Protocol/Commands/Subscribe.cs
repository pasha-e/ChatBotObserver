using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerBotObserver.PostLink.Protocol.Commands
{
    public class Subscribe
    {
        public string command => "subscribe";

        public string[] types { get; set; }

        public int[] fromUsers { get; set; }

        public int[] fromConferences { get; set; }
    }
}
