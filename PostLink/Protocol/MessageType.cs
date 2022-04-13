using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerBotObserver.PostLink.Protocol
{
    public enum MessageType
    {
        own,
        ownInConf,
        person,
        conference,
        ownFiles,
        ownInConfFiles,
        personFiles,
        conferenceFiles,
        userStatus,
        userCard,
        userCoords,
        binaryData,
        messageState,
        fileState
    }
}
