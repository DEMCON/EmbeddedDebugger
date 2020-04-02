using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbeddedDebugger.DebugProtocol.Enums;

namespace EmbeddedDebugger.DebugProtocol.Messages
{
    public class ConfigChannelMessage : ApplicationMessage
    {
        public override Command Command => Command.ConfigChannel;

        public byte ChannelId { get; set; }

        public ChannelMode? Mode { get; set; }

        public uint? Offset { get; set; }
        public byte? Control { get; set; }
        public int? Size { get; set; }

        public ConfigChannelMessage()
        {
        }

        public ConfigChannelMessage(ProtocolMessage msg) : this(msg.CommandData)
        {
        }

        public ConfigChannelMessage(byte[] payload)
        {
            if (payload.Length < 1)
            {
                throw new ArgumentException("Message too short for Config Channel Message");
            }

            ChannelId = payload[0];

            if (payload.Length >= 2)
            {
                Mode = (ChannelMode)payload[1];
            }

            if (payload.Length >= 8)
            {
                Offset = BitConverter.ToUInt32(payload.Skip(2).Take(4).ToArray(), 0);
                Control = payload[6];
                Size = payload[7];
            }
        }


        public override byte[] ToBytes()
        {
            List<byte> data = new List<byte>
            {
                ChannelId
            };

            if (Mode.HasValue)
            {
                data.Add((byte)Mode.Value);
            }

            if (Offset.HasValue && Control.HasValue && Size.HasValue)
            {
                data.AddRange(BitConverter.GetBytes(Offset.Value));
                data.Add(Control.Value);
                data.Add((byte)Size.Value);
            }

            return data.ToArray();
        }
    }
}
