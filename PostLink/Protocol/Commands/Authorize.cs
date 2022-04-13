using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessengerBotObserver.Properties;

namespace MessengerBotObserver.PostLink.Protocol.Commands
{
    public class Authorize
    {
        public string command => "authorizationRequest";

        public string clientDescription => Settings.Default.PostLinkClientDescription;

        public string uuid => Settings.Default.PostLinkBotUuid;
    }
}
