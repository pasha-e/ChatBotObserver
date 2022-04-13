using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using MessengerBotObserver.Diagnostics;
using MessengerBotObserver.PostLink.PacketAssembling;
using MessengerBotObserver.PostLink.Protocol;
using MessengerBotObserver.PostLink.Protocol.Commands;
using MessengerBotObserver.Properties;
using Newtonsoft.Json;
using ConnectionState = MessengerBotObserver.PostLink.Protocol.Commands.ConnectionState;

namespace MessengerBotObserver.PostLink
{
    public class PostLinkMessagesController : ILogging
    {
        #region Fields

        private readonly PostLinkConnection _connection;

        private List<int> _contacts;

        private readonly System.Timers.Timer _requestingTimer;

        private int _localUserId = -1;

        #endregion

        #region Construction

        public PostLinkMessagesController(PostLinkConnection connection, PacketAssembler packetAssembler)
        {
            _connection = connection;
            _connection.ConnectedChanged += ConnectionOnConnectedChanged;

            packetAssembler.PacketAssembled += PacketAssemblerOnPacketAssembled;

            _requestingTimer = new System.Timers.Timer(Settings.Default.PostLinkRequestingAuthorizationInterval);
            _requestingTimer.Elapsed += RequestingTimerOnElapsed;
            _requestingTimer.AutoReset = true;
        }

        #endregion

        #region Public properties

        public int LocalUserId => _localUserId;

        #endregion

        #region Public methods

        public override string ToString()
        {
            return nameof(PostLinkMessagesController);
        }

        public bool Authorize()
        {
            try
            {
                var command = new Authorize();

                _connection.Send(new PostLinkPacket(JsonConvert.SerializeObject(command), new byte[0]).GetFullArray());
                LogMessageReceived?.Invoke(ToString(), new LogMessage(LogMessageType.Information, $"Authorization requested. With description {command.clientDescription} and uuid {command.uuid}"));
                //LogMessageReceived?.Invoke(ToString(), new LogMessage(LogMessageType.Information, $"Authorization requested. With description {command.clientDescription} "));
                return true;
            }
            catch (Exception exception)
            {
                LogMessageReceived?.Invoke(ToString(), new LogMessage(exception));
                return false;
            }
        }

        public bool SubscribeToMessages()
        {
            try
            {
                var types = new[] { MessageType.person.ToString(),  MessageType.conference.ToString() };
                var command = new Subscribe
                {
                    types = types
                };
                
                _connection.Send(new PostLinkPacket(JsonConvert.SerializeObject(command), new byte[0]).GetFullArray());
                LogMessageReceived?.Invoke(ToString(), new LogMessage(LogMessageType.Information, $"Subscribed to messages: {string.Join(", ", types)}"));
                return true;
            }
            catch (Exception exception)
            {
                LogMessageReceived?.Invoke(ToString(), new LogMessage(exception));
                return false;
            }
        }

        public bool SendMessage(Guid id, string message)
        {
            try
            {
                _connection.Send(new PostLinkPacket(message, new byte[0]).GetFullArray());

                LogMessageReceived?.Invoke(ToString(), new LogMessage(LogMessageType.Debug, $"Message '{id}' sent out"));
                return true;
            }
            catch (Exception exception)
            {
                LogMessageReceived?.Invoke(ToString(), new LogMessage(exception));
                return false;
            }
        }

        #endregion

        #region Handlers

        private void ConnectionOnConnectedChanged(bool connected)
        {
            try
            {
                _localUserId = -1;

                if (connected)
                {
                    _requestingTimer.Start();
                }
                else
                {
                    _requestingTimer.Stop();
                }
            }
            catch (Exception exception)
            {
                LogMessageReceived?.Invoke(ToString(), new LogMessage(exception));
            }
        }

        private void RequestingTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Authorize();
            }
            catch (Exception exception)
            {
                LogMessageReceived?.Invoke(ToString(), new LogMessage(exception));
                _requestingTimer.Start();
            }
        }

        private void PacketAssemblerOnPacketAssembled(PostLinkPacket packet)
        {
            try
            {
                LogMessageReceived?.Invoke(ToString(), new LogMessage(LogMessageType.Debug, $"Packet received, {packet.Header}"));

                var message = Encoding.UTF8.GetString(packet.MessageData);
                var command = JsonConvert.DeserializeObject<Command>(message);

                LogMessageReceived?.Invoke(ToString(), new LogMessage(LogMessageType.Debug, $"Command '{command.command}' received"));

                ProcessMessage(message, command);
            }
            catch (Exception exception)
            {
                LogMessageReceived?.Invoke(ToString(), new LogMessage(exception));
            }
        }

        #endregion

        #region Private methods

        
        private void ProcessMessage(string message, Command command)
        {
            try
            {
                switch (command.command)
                {
                    case "messages":
                        UserMessagesListReceived?.Invoke(JsonConvert.DeserializeObject<UserMessagesList>(message));
                        break;
                    case "authorizationGranted":
                        ProcessAuthorizationInfo(message);
                        break;
                    case "sendMessageResult":
                        SendMessageResultReceived?.Invoke(JsonConvert.DeserializeObject<SendMessageResult>(message));
                        break;
                    case "connectionState":
                        ProcessConnectionState(message);
                        break;
                }
            }
            catch (Exception exception)
            {
                LogMessageReceived?.Invoke(ToString(), new LogMessage(exception));
            }
        }

        private void ProcessConnectionState(string message)
        {
            try
            {
                var connectionStateInfo = JsonConvert.DeserializeObject<ConnectionState>(message);

                LogMessageReceived?.Invoke(ToString(), new LogMessage(LogMessageType.Information, $"Message {connectionStateInfo.command} received.  {connectionStateInfo.ToString()}."));
            }
            catch (Exception exception)
            {
                LogMessageReceived?.Invoke(ToString(), new LogMessage(exception));
            }
        }
        
        private string ConvertToStringDescription(int state)
        {
            string result = "unknown";

            switch (state)
            {
                case 0:
                    result = "receiving";
                    break;
                case 1:
                    result = "received, but not downloaded";
                    break;
                case 2:
                    result = "downloaded";
                    break;
                case 3:
                    result = "recipient refused to download";
                    break;
                case 4:
                    result = "canceled by receiver or error downloading";
                    break;
            }

            return result;

        }

        private void ProcessAuthorizationInfo(string message)
        {
            _requestingTimer.Stop();
            var authorizationResult = JsonConvert.DeserializeObject<AuthorizationGranted>(message);
            _localUserId = authorizationResult.userId;
            LogMessageReceived?.Invoke(ToString(), new LogMessage(LogMessageType.Information, $"Authorized. User id: {authorizationResult.userId}"));
            AuthorizationGrantedReceived?.Invoke(authorizationResult);

            SubscribeToMessages();
        }

        

        #endregion

        #region Events

        public event Action<string, LogMessage> LogMessageReceived;

        public event Action<AuthorizationGranted> AuthorizationGrantedReceived;

        public event Action<SendMessageResult> SendMessageResultReceived;

        public event Action<UserMessagesList> UserMessagesListReceived;

        #endregion
    }
}
