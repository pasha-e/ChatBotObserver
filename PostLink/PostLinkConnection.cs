using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MessengerBotObserver.Diagnostics;
using MessengerBotObserver.Properties;

namespace MessengerBotObserver.PostLink
{
    public class PostLinkConnection : ILogging, IDisposable
    {
        #region Fields

        private Socket _socket;

        private readonly Timer _reconnectionTimer;

        private readonly byte[] _buffer = new byte[1024 * 1024];

        private bool _connected;

        private bool _connecting;

        private bool _disposed;

        private readonly int _waitTime = 1000;

        private object _receiveLocker = new object();

        #endregion

        #region Construction

        public PostLinkConnection()
        {
            _waitTime = Settings.Default.WaitRecieveTime;

            _reconnectionTimer = new Timer(Settings.Default.PostLinkReconnectionInterval);
            _reconnectionTimer.Elapsed += ReconnectionTimerOnElapsed;
            _reconnectionTimer.AutoReset = false;
        }

        #endregion

        #region Public properties

        public bool Connected
        {
            get => _connected;
            private set
            {
                if (_connected == value)
                    return;

                _connected = value;
                LogMessageReceived?.Invoke(ToString(), new LogMessage(LogMessageType.Information, _connected ? "Connected" : "Disconnected"));
                ConnectedChanged?.Invoke(_connected);
            }
        }

        public bool Connecting
        {
            get => _connecting;
            private set
            {
                if (_connecting == value)
                    return;

                _connecting = value;

                if (_connecting)
                {
                    LogMessageReceived?.Invoke(ToString(), new LogMessage(LogMessageType.Information, "Connecting..."));
                }
            }
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return nameof(PostLinkConnection);
        }

        public void Send(byte[] data)
        {
            if (_disposed)
                throw new InvalidOperationException("Object disposed");

            if (!Connected)
                throw new InvalidOperationException("Not connected to PostLink");

            _socket.Send(data);

#if EXTRADEBUG
            Console.WriteLine($"Sent out string: {Encoding.UTF8.GetString(data)}{Environment.NewLine}");
#endif
        }

        public void Connect()
        {
            try
            {
                if (_disposed)
                    throw new InvalidOperationException("Object disposed");

                Connected = false;
                Connecting = true;

                _socket?.Dispose();
                _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                //
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 5000);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(true, 1));
                SetKeepAliveOption(_socket);
                _socket.ReceiveBufferSize = 8192 * 2;
                //
                _socket.Connect(Settings.Default.PostLinkAddress, Settings.Default.PostLinkPort);


                Connected = true;


                Task.Factory.StartNew(() =>
                {
                    System.Threading.Thread.Sleep(3000);
                    BeginReceive();
                });
            }
            catch (SocketException exception)
            {
                LogMessageReceived?.Invoke(ToString(), new LogMessage(LogMessageType.Warning, exception.Message));
                Connected = false;
                _reconnectionTimer.Start();
            }
            catch (Exception exception)
            {
                LogMessageReceived?.Invoke(ToString(), new LogMessage(exception));
                Connected = false;
                _reconnectionTimer.Start();
            }
            finally
            {
                Connecting = false;
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _reconnectionTimer?.Dispose();
            _socket?.Dispose();
        }

        #endregion

        #region Private methods

        private void SetKeepAliveOption(Socket s)
        {
            uint onoff = 1;
            uint keepalivetime = 25000;
            uint keepaliveinterval = 500;
            BinaryWriter bw = new BinaryWriter(new MemoryStream());
            bw.Write(onoff);
            bw.Write(keepalivetime);
            bw.Write(keepaliveinterval);
            byte[] inbuf = new byte[12];
            byte[] outbuf = { 0, 0, 0, 0 };
            bw.Seek(0, SeekOrigin.Begin);
            bw.BaseStream.Read(inbuf, 0, inbuf.Length);
            int ccKeepAlive = unchecked((int)0x98000004);
            s.IOControl(ccKeepAlive, inbuf, outbuf);
        }

        private void ReconnectionTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (_disposed)
                return;

            Connect();
        }

        private void BeginReceive()
        {
            try
            {
                //System.Threading.Thread.Sleep(_waitTime); //1000
                //_socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, EndReceive, _socket);

                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, EndReceive, null);
            }
            catch (Exception exception)
            {
                LogMessageReceived?.Invoke(ToString(), new LogMessage(exception));
                Connected = false;
                _reconnectionTimer.Start();
            }
        }

        private void EndReceive(IAsyncResult ar)
        {
            lock (_receiveLocker)
            {
                try
                {
                    //var socket = (Socket) ar.AsyncState;
                    var len = _socket.EndReceive(ar);

                    var data = new byte[len];
                    Array.Copy(_buffer, data, len);

                    if (len > 0)
                    {
#if EXTRADEBUG
                    Console.WriteLine($"Received string: {Encoding.UTF8.GetString(data)}{Environment.NewLine}");
#endif
                        LogMessageReceived?.Invoke(ToString(),
                            new LogMessage(LogMessageType.Debug, $"Data received, length={len}"));

                        DataReceived?.Invoke(data);
                    }

                    //BeginReceive();
                    _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, EndReceive, null);
                }
                catch (InvalidOperationException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    LogMessageReceived?.Invoke(ToString(), new LogMessage(ex));
                    Connected = false;
                    _reconnectionTimer.Start();
                }
            }
        }

        #endregion

        #region Events

        public event Action<string, LogMessage> LogMessageReceived;

        public event Action<byte[]> DataReceived;

        public event Action<bool> ConnectedChanged;

        #endregion
    }
}
