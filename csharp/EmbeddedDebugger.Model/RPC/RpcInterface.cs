/*
Embedded Debugger PC Application which can be used to debug embedded systems at a high level.
Copyright (C) 2019 DEMCON advanced mechatronics B.V.

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.IO;
using System.Net;
using System.Threading;
using CookComputing.XmlRpc;
using EmbeddedDebugger.DebugProtocol.EmbeddedConfiguration;

namespace EmbeddedDebugger.Model.RPC
{
    public class RpcInterface
    {
        private string hostName;
        private int port;
        private HttpListener listener;
        private RpcResolver resolver;
        private bool isRunning;
        private Thread myThread;

        #region Properties
        public string HostName { get => hostName; set => hostName = value; }
        public int Port { get => port; set => port = value; }
        public bool IsRunning { get => isRunning; }
        #endregion

        public RpcInterface(ModelManager mm, ConnectionManager dp)
        {
            hostName = "127.0.0.1";
            // TODO Add port
            //port = Properties.Settings.Default.RPCPort;
            resolver = new RpcResolver(mm, dp);
        }

        public void Start()
        {
            if (isRunning) return;
            listener = new HttpListener();
            listener.Prefixes.Add($"http://{hostName}:{port}/");
            listener.Start();
            isRunning = true;
            myThread = new Thread(DoWork) { IsBackground = true, };
            myThread.Start();
        }

        public void Stop()
        {
            if (!isRunning) return;
            isRunning = false;
            myThread.Abort();
            listener.Stop();
        }

        private void DoWork()
        {
            while (isRunning)
            {
                HttpListenerContext context = listener.GetContext();
                ListenerService svc = resolver;
                svc.ProcessRequest(context);
            }
        }
    }


    public abstract class ListenerService : XmlRpcHttpServerProtocol
    {
        public virtual void ProcessRequest(HttpListenerContext RequestContext)
        {
            try
            {
                IHttpRequest req = new ListenerRequest(RequestContext.Request);
                IHttpResponse resp = new ListenerResponse(RequestContext.Response);
                HandleHttpRequest(req, resp);
                RequestContext.Response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                // "Internal server error"
                RequestContext.Response.StatusCode = 500;
                RequestContext.Response.StatusDescription = ex.Message;
            }
        }
    }
    public class ListenerRequest : IHttpRequest
    {
        public ListenerRequest(HttpListenerRequest request)
        {
            this.request = request;
        }

        public Stream InputStream
        {
            get { return request.InputStream; }
        }

        public string HttpMethod
        {
            get { return request.HttpMethod; }
        }

        private HttpListenerRequest request;
    }

    public class ListenerResponse : IHttpResponse
    {
        public ListenerResponse(HttpListenerResponse response)
        {
            this.response = response;
        }

        string IHttpResponse.ContentType
        {
            get { return response.ContentType; }
            set { response.ContentType = value; }
        }

        TextWriter IHttpResponse.Output
        {
            get { return new StreamWriter(response.OutputStream); }
        }

        Stream IHttpResponse.OutputStream
        {
            get { return response.OutputStream; }
        }

        int IHttpResponse.StatusCode
        {
            get { return response.StatusCode; }
            set { response.StatusCode = value; }
        }

        string IHttpResponse.StatusDescription
        {
            get { return response.StatusDescription; }
            set { response.StatusDescription = value; }
        }

        public long ContentLength { set => response.ContentLength64 = value; }
        public bool SendChunked { get => response.SendChunked; set => response.SendChunked = value; }

        private HttpListenerResponse response;
    }
}
