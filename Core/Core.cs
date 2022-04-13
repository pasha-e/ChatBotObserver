using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessengerBotObserver.Diagnostics;
using MessengerBotObserver.PostLink;
using MessengerBotObserver.PostLink.PacketAssembling;
using MessengerBotObserver.PostLink.Protocol;
using MessengerBotObserver.PostLink.Protocol.Commands;

namespace MessengerBotObserver.Core
{
    public class Core
    {
        FileLogger _logger;

        private PostLinkConnection _connectionToPostLink;
        private PacketAssembler _assembler;
        private PostLinkMessagesController _postLinkMessagesController;

        public Core()
        {
            _logger = new FileLogger();
        }

        public void Start()
        {
            _logger.AddMessage(nameof(Core), new LogMessage(LogMessageType.Information, "Start"));

            _connectionToPostLink = new PostLinkConnection();
            _assembler = new PacketAssembler(_connectionToPostLink);
            _postLinkMessagesController = new PostLinkMessagesController(_connectionToPostLink, _assembler);

            _postLinkMessagesController.UserMessagesListReceived += _postLinkMessagesController_UserMessagesListReceived;

            _logger.AddLoggingModule(_connectionToPostLink);
            _logger.AddLoggingModule(_assembler);
            _logger.AddLoggingModule(_postLinkMessagesController);


            Task.Factory.StartNew(() =>
            {
                _connectionToPostLink.Connect();
            });
        }

        private void _postLinkMessagesController_UserMessagesListReceived(UserMessagesList messages)
        {
            try
            {
                if (messages.list != null && messages.list.Length > 0)
                {
                    foreach (var message in messages.list)
                    {
                        if (message.senderType == MessageType.person.ToString() ||
                            message.senderType == MessageType.conference.ToString())
                        {
                            _logger.AddMessage(nameof(Core), new LogMessage(LogMessageType.Information, $"{message.senderName} : {message.message} "));
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.AddMessage(nameof(Core), new LogMessage(exception));
            }
        }

    }
}
