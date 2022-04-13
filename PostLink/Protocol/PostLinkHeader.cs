using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerBotObserver.PostLink.Protocol
{
    public struct PostLinkHeader
    {
        public PostLinkHeader(int messageLength, long binaryDataLength)
        {
            MessageLength = messageLength;
            BinaryDataLength = binaryDataLength;
        }

        public PostLinkHeader(byte[] data)
        {
            MessageLength = BitConverter.ToInt32(data.Take(4).Reverse().ToArray(), 0);
            BinaryDataLength = BitConverter.ToInt64(data.Skip(4).Take(8).Reverse().ToArray(), 0);
        }

        public int MessageLength { get; set; }

        public long BinaryDataLength { get; set; }

        public byte[] GetFullArray()
        {
            byte[] data = new byte[HeaderLength];

            int pointer = 0;

            Array.Copy(BitConverter.GetBytes(MessageLength).Reverse().ToArray(), 0, data, pointer, 4);
            pointer += 4;

            Array.Copy(BitConverter.GetBytes(BinaryDataLength).Reverse().ToArray(), 0, data, pointer, 8);

            return data;
        }

        public override string ToString()
        {
            return $"message length = {MessageLength}, binary data length = {BinaryDataLength}";
        }

        public static int HeaderLength => 12;
    }
}
