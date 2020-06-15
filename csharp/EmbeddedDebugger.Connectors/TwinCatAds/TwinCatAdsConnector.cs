using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using EmbeddedDebugger.Connectors.CustomEventArgs;
using EmbeddedDebugger.Connectors.Interfaces;
using NLog;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;

namespace EmbeddedDebugger.Connectors.TwinCatAds
{
    public class TwinCatAdsConnector : Connector, IDisposable
    {
        public TcAdsClient Client { get; }

        public TwinCatAdsConnector()
        {
            this.Name = "TwinCAT Ads";

            this.Client = new TcAdsClient();
        }

        public string HostName { get; set; }
        public int Port { get; set; }
        public override string Name { get; }
        public override bool IsConnected { get; }
        public override event EventHandler<BytesReceivedEventArgs> MessageReceived;
        public override event EventHandler UnexpectedDisconnect;
        public override event EventHandler HasConnected;
        public override bool? ShowDialog()
        {
            // Set the current settings to the settings form
            TwinCatAdsSettingsWindow tcasw = new TwinCatAdsSettingsWindow
            {
                HostName = this.HostName,
                Port = this.Port,
            };

            bool? dr = tcasw.ShowDialog();
            // Fetch the new settings from the form
            if (dr == true)
            {
                this.HostName = tcasw.HostName;
                this.Port = tcasw.Port;
            }
            return dr;
        }

        public override bool Connect()
        {
            // If the hostname is not set, show the settings form
            if (this.HostName == null && this.ShowDialog() == false)
            {
                return false;

            }

            this.Client.ConnectionStateChanged += this.Client_ConnectionStateChanged;
            this.Client.AdsStateChanged += this.Client_AdsStateChanged;
            this.Client.AmsRouterNotification += this.Client_AmsRouterNotification;

            try
            {
                this.Client.Connect(this.HostName, this.Port);
            }
            catch (Exception _)
            {
                return false;
            }

            this.SymbolLoader = this.Client.CreateSymbolLoader(this.Client.Session, SymbolLoaderSettings.DefaultDynamic);
            return true;
        }

        private void Client_ConnectionStateChanged(object sender, TwinCAT.ConnectionStateChangedEventArgs e)
        {
        }

        private void Client_AmsRouterNotification(object sender, AmsRouterNotificationEventArgs e)
        {
        }

        private void Client_AdsStateChanged(object sender, AdsStateChangedEventArgs e)
        {
        }

        public override void Disconnect()
        {
            this.Client.Disconnect();
        }

        public override void SendMessage(byte[] msg)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return this.Name;
        }

        public override bool AsServer { get; set; }
        [XmlIgnore]
        public ISymbolLoader SymbolLoader { get; private set; }

        public override void ReceiveMessage(byte[] msg)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
