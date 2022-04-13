using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerBotObserver.PostLink.Protocol
{
    public struct PostLinkPacket
    {
        public PostLinkPacket(PostLinkHeader header, byte[] messageDataAndBinaryData)
        {
            if (messageDataAndBinaryData.Length != header.MessageLength + header.BinaryDataLength)
            {
                throw new ArgumentException($"Invalid data length in header. Header message length = {header.MessageLength}, header binary data length = {header.BinaryDataLength}, data length = {messageDataAndBinaryData.Length} ");
            }

            Header = header;
            MessageData = new byte[Header.MessageLength];
            BinaryData = new byte[Header.BinaryDataLength];

            Array.Copy(messageDataAndBinaryData, 0, MessageData, 0, MessageData.Length);
            Array.Copy(messageDataAndBinaryData, MessageData.Length, BinaryData, 0, BinaryData.Length);
        }

        public PostLinkPacket(byte[] allData)
        {
            if (allData.Length <= PostLinkHeader.HeaderLength)
            {
                throw new ArgumentException("Data length should be greater than header length");
            }

            Header = new PostLinkHeader(allData);
            MessageData = new byte[Header.MessageLength];
            BinaryData = new byte[Header.BinaryDataLength];

            Array.Copy(allData, PostLinkHeader.HeaderLength, MessageData, 0, MessageData.Length);
            Array.Copy(allData, MessageData.Length + PostLinkHeader.HeaderLength, BinaryData, 0, BinaryData.Length);
        }

        public PostLinkPacket(PostLinkHeader header, byte[] messageData, byte[] binaryData)
        {
            if (header.MessageLength != messageData.Length || header.BinaryDataLength != binaryData.Length)
            {
                throw new ArgumentException("Invalid data length in header");
            }

            Header = header;
            MessageData = messageData;
            BinaryData = binaryData;
        }

        public PostLinkPacket(string message, byte[] binaryData)
        {
            MessageData = Encoding.UTF8.GetBytes(message);
            BinaryData = binaryData;

            Header = new PostLinkHeader(MessageData.Length, BinaryData.Length);
        }

        public PostLinkPacket(string message) : this(message, new byte[0])
        {
        }

        public PostLinkHeader Header { get; }

        public byte[] MessageData { get; }

        public byte[] BinaryData { get; }

        public byte[] GetFullArray()
        {
            var headerData = Header.GetFullArray();

            byte[] array = new byte[headerData.Length + MessageData.Length + BinaryData.Length];

            Array.Copy(headerData, 0, array, 0, headerData.Length);
            Array.Copy(MessageData, 0, array, headerData.Length, MessageData.Length);
            Array.Copy(BinaryData, 0, array, headerData.Length + MessageData.Length, BinaryData.Length);

            return array;
        }
    }
}
