using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbeddedDebugger.DebugProtocol.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmbeddedDebugger.UnitTests.Model
{
    [TestClass]
    public class VersionMessageTests
    {
        [TestMethod]
        public void SerializationRoundTrip()
        {
            VersionMessage m1 = new VersionMessage()
            {
                Name = "Fubar1",
                SerialNumber = "1337.42",
                ApplicationVersion = new Version(1, 3, 5),
                ProtocolVersion = new Version(0, 1, 2000)
            };
            var payload = m1.ToBytes();
            VersionMessage m2 = new VersionMessage(payload);

            Assert.AreEqual(m1.Name, m2.Name);
            Assert.AreEqual(m1.SerialNumber, m2.SerialNumber);
            Assert.AreEqual(m1.ApplicationVersion, m2.ApplicationVersion);
            Assert.AreEqual(m1.ProtocolVersion, m2.ProtocolVersion);
        }
    }
}

