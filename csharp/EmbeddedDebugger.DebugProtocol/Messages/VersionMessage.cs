using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EmbeddedDebugger.DebugProtocol.Enums;

namespace EmbeddedDebugger.DebugProtocol.Messages
{
    public class VersionMessage : ApplicationMessage
    {
        public Version ProtocolVersion { get; set; }

        public Version ApplicationVersion { get; set; }

        public String Name { get; set; }

        public String SerialNumber { get; set; }

        public override Command Command => Command.GetVersion;

        public VersionMessage()
        {
            this.ProtocolVersion = new Version();
            this.ApplicationVersion = new Version();
        }

        public VersionMessage(ProtocolMessage msg) : this(msg.CommandData)
        {
        }

        public VersionMessage(byte[] payload)
        {
            // Check if message is too small
            if (payload.Length < 9)
            {
                throw new ArgumentException("Message too short for Version Message");
            }

            // Extract fields from message command data:
            byte[] protVersion = payload.Take(4).ToArray();
            byte[] appVersion = payload.Skip(4).Take(4).ToArray();
            byte name_length = payload.Skip(8).Take(1).ToArray()[0];
            string name = BytesToString(payload.Skip(9).Take(name_length).ToArray());

            string serialNumber = "";
            int serial_number_offset = 9 + name_length;

            if (payload.Length > serial_number_offset)
            {
                byte serial_number_length = payload.Skip(serial_number_offset).Take(1).ToArray()[0];
                serialNumber = BytesToString(payload.Skip(serial_number_offset + 1).Take(serial_number_length).ToArray());
            }

            this.ApplicationVersion = BytesToVersion(appVersion);
            this.ProtocolVersion = BytesToVersion(protVersion);
            this.Name = name;
            this.SerialNumber = serialNumber;
        }

        private static string BytesToString(byte[] data)
        {
            return Encoding.UTF8.GetString(data).Replace("\n", "").Replace("\r", "").Replace("\t", "");
        }

        private static byte[] VersionToBytes(Version version)
        {
            List<byte> data = new List<byte>
            {
                (byte)version.Major,
                (byte)version.Minor,
                (byte)version.Build,
                (byte)(version.Build >> 8)
            };
            return data.ToArray();
        }

        private static Version BytesToVersion(byte[] version_data)
        {
            return new Version(version_data[0], version_data[1], version_data[3] << 8 | version_data[2]);
        }

        public override byte[] ToBytes()
        {
            List<byte> data = new List<byte>();
            data.AddRange(VersionToBytes(this.ProtocolVersion));
            data.AddRange(VersionToBytes(this.ApplicationVersion));

            if (!string.IsNullOrEmpty(Name))
            {
                data.Add((byte)Name.Length);
                data.AddRange(Encoding.ASCII.GetBytes(Name));
            }

            if (!string.IsNullOrEmpty(SerialNumber))
            {
                data.Add((byte)SerialNumber.Length);
                data.AddRange(Encoding.ASCII.GetBytes(SerialNumber));
            }

            return data.ToArray();
        }
    }
}
