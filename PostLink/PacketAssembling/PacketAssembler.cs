using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessengerBotObserver.Diagnostics;
using MessengerBotObserver.PostLink.Protocol;

namespace MessengerBotObserver.PostLink.PacketAssembling
{
    public class PacketAssembler : ILogging
    {
        #region Fields

        private SortedList<long, byte[]> _buffer = new SortedList<long, byte[]>();

        private List<byte> _packetSizeBuffer = new List<byte>();
        private List<byte> _packetContentBuffer = new List<byte>();
        private long _totalBytesReceived = 0;
        private long _totalSumUnpacked = 0;
        private long _totalUnpacked = 0;
        private long _packetSize = 0;


        private long _pointer = 0;

        private readonly object _sync = new object();

        #endregion

        #region Construction

        public PacketAssembler(PostLinkConnection connection)
        {
            lock (_sync)
            {
                connection.ConnectedChanged += ConnectionOnConnectedChanged;
                connection.DataReceived += ConnectionOnDataReceived;
            }
        }

        #endregion

        #region Public properties

        #endregion

        #region Public methods

        public override string ToString()
        {
            return nameof(PacketAssembler);
        }

        #endregion

        #region Handlers

        private void ConnectionOnConnectedChanged(bool connected)
        {
            lock (_sync)
            {
                _buffer = new SortedList<long, byte[]>();
                _pointer = 0;

                _packetSizeBuffer.Clear(); //начинаем собирать пакет с нуля
                _packetContentBuffer.Clear(); //начинаем собирать пакет с нуля

                LogMessageReceived?.Invoke(ToString(), new LogMessage(LogMessageType.Information, $"Reset"));
            }
        }
        

        public void ConnectionOnDataReceived(byte[] curReceivedBuffer)
        {
            lock (_sync)
            {
                var curReceivedCount = curReceivedBuffer.Length;
                _totalBytesReceived += curReceivedCount;

                try
                {

                    //общий принцип - наполняем заголов первыми полученными 12 байтами. Затем из заголовка получаем длину L - следующие 
                    //L байт записываем в тело пакета. После этого пакет собран, начинаем следующий
                    foreach (var curByte in curReceivedBuffer)
                    {
                        if (_packetSizeBuffer.Count < PostLinkHeader.HeaderLength) //если заголовок еще не заполнен
                        {
                            _packetSizeBuffer.Add(curByte); //добавляем в заголовок


                            if (_packetSizeBuffer.Count == PostLinkHeader.HeaderLength
                            ) //как только заполнили заголовок - определяем длину тела пакета
                            {
                                PostLinkHeader header = new PostLinkHeader(_packetSizeBuffer.ToArray());

                                _packetSize = header.MessageLength + header.BinaryDataLength;

                            }
                        }
                        else //байт не относится к заголовку пакета
                        {
                            if (_packetSize != 0) //тело пакета имеет некоторую длину
                            {
                                _packetContentBuffer.Add(curByte); //добавляем в тело пакета

                                if (_packetContentBuffer.Count == _packetSize) //как только набрали тело пакета
                                {
                                    var packet = _packetContentBuffer.ToArray(); //получаем тело

                                    PostLinkHeader header = new PostLinkHeader(_packetSizeBuffer.ToArray());

                                    var postLinkPacket = new PostLinkPacket(header, packet);

                                    _totalUnpacked += packet.Length; //считаем, сколько байт мы получили
                                    _totalSumUnpacked +=
                                        packet.Length + PostLinkHeader.HeaderLength; //считаем, сколько байт мы получили

                                    LogMessageReceived?.Invoke(ToString(),
                                        new LogMessage(LogMessageType.Debug,
                                            $"Packet assembled, length={PostLinkHeader.HeaderLength + packet.Length}, header: MessageLength =  {header.MessageLength}, BinaryDataLength = {header.BinaryDataLength} "));
                                    PacketAssembled?.Invoke(postLinkPacket);

                                    _packetSizeBuffer.Clear(); //начинаем собирать пакет с нуля
                                    _packetContentBuffer.Clear(); //начинаем собирать пакет с нуля
                                }

                            }
                            else
                            {
                                _packetSizeBuffer.Clear(); //начинаем собирать пакет с нуля
                                _packetContentBuffer.Clear(); //начинаем собирать пакет с нуля
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessageReceived?.Invoke(ToString(), new LogMessage(LogMessageType.Error, $"Exception {ex}"));
                    //throw new ApplicationException($"Packet assembler error.");
                }
            }
        }



        #endregion

        #region Private methods
 
        #endregion

        #region Events

        public event Action<PostLinkPacket> PacketAssembled;

        public event Action<string, LogMessage> LogMessageReceived;

        #endregion
    }
}
