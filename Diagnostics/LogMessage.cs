using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerBotObserver.Diagnostics
{
    public class LogMessage
    {
        public LogMessage(LogMessageType type, string message)
        {
            Type = type;
            Message = message;
        }

        public LogMessage(Exception ex)
        {
            Type = LogMessageType.Error;
            Message = ex.Message;
            ExtendedMessage = ex.ToString();
        }

        public LogMessageType Type { get; set; }

        public string Message { get; set; }

        public string ExtendedMessage { get; set; }
    }
}
