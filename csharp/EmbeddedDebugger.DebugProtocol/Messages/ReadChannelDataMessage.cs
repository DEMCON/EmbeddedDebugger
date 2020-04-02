using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbeddedDebugger.DebugProtocol.Enums;

namespace EmbeddedDebugger.DebugProtocol.Messages
{
    public class ReadChannelDataMessage : ApplicationMessage
    {
        public override Command Command => Command.ReadChannelData;

        public uint TimeStamp { get; set; }

        public ushort Mask { get; set; }

        public byte[] Data { get; set; }

        public ReadChannelDataMessage()
        {
        }

        public ReadChannelDataMessage(ProtocolMessage msg) : this(msg.CommandData)
        {
        }

        public ReadChannelDataMessage(byte[] payload)
        {
            if (payload.Length < 5)
            {
                throw new ArgumentException("Message too short for Read Channel Data Message");
            }
            byte[] timeArray = payload.Take(3).ToArray();
            TimeStamp = (uint)((0x00 << 24) | (timeArray[2] << 16) | (timeArray[1] << 8) | timeArray[0]);
            Mask = BitConverter.ToUInt16(payload.Skip(3).Take(2).ToArray(), 0);
            Data = payload.Skip(5).ToList().ToArray();
        }

        public override byte[] ToBytes()
        {
            List<byte> data = new List<byte>
                {
                    (byte)(TimeStamp),
                    (byte)(TimeStamp >> 8),
                    (byte)(TimeStamp >> 16)
                };
            data.Add((byte)Mask);
            data.Add((byte)(Mask >> 8));
            data.AddRange(Data);
            return data.ToArray();
        }
    }
}
