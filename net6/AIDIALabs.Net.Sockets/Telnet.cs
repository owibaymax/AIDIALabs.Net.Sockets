using System.Net.Sockets;

namespace AIDIALabs.Net.Sockets
{
    public class Telnet
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public int AlternativePort { get; set; }
        public int TimeOut { get; set; }

        public delegate void TelnetCompletedEventHandler(TelnetConnectivityResult e);
        public event TelnetCompletedEventHandler TelnetCompleted;

        private TcpClient tcpClient = null;

        public Telnet()
        {

        }

        private void RaiseTelnetCompletedEvent(bool isConnectionOK, bool? isNetworkOK = null, Exception ex = null)
        {
            if (TelnetCompleted != null)
            {
                TelnetCompleted(new TelnetConnectivityResult
                {
                    IsAlternativePortConnectivityOK = isNetworkOK,
                    IsPortConnectivityOK = isConnectionOK,
                    Exception = ex
                });
            }
        }

        public void ConnectAsync(string ip, int port, int timeOut)
        {
            IP = ip;
            Port = port;
            TimeOut = timeOut;
            tcpClient = new TcpClient();

            try
            {
                var connectionStatusTelentCallback = new AsyncCallback(ConnectionStatusTelentResult);
                var arConnectionStatus = tcpClient.BeginConnect(IP, Port, connectionStatusTelentCallback, tcpClient);
                var successConnectionStatus = arConnectionStatus.AsyncWaitHandle.WaitOne(TimeOut * 1000);
                if (!successConnectionStatus)
                {
                    RaiseTelnetCompletedEvent(false);
                }

            }
            catch (Exception ex)
            {
                RaiseTelnetCompletedEvent(false, null, ex);
            }
        }

        private void ConnectionStatusTelentResult(IAsyncResult asyncResult)
        {
            TcpClient oTcpClient = asyncResult.AsyncState as TcpClient;
            try
            {
                oTcpClient.EndConnect(asyncResult);
                bool tcpResult = oTcpClient.Connected;
                if (tcpResult)
                {
                    RaiseTelnetCompletedEvent(true);
                }
                else
                {
                    throw new Exception(Port + " not listening.");
                }
            }
            catch (Exception ex)
            {
                RaiseTelnetCompletedEvent(false, null, ex);
            }
        }

        public void ConnectAsync(string ip, int port, int alternativePort, int timeOut)
        {
            IP = ip;
            Port = port;
            TimeOut = timeOut;
            AlternativePort = alternativePort;
            tcpClient = new TcpClient();

            try
            {
                var communicationStatusTelentCallback = new AsyncCallback(CommunicationStatusTelentResult);
                var arCommunicationStatus = tcpClient.BeginConnect(IP, Port, communicationStatusTelentCallback, tcpClient);
                var successCommunicationStatus = arCommunicationStatus.AsyncWaitHandle.WaitOne(TimeOut * 1000);
                if (!successCommunicationStatus)
                {
                    var networkStatusTelentCallback = new AsyncCallback(NetworkStatusTelentResult);
                    var arNetworkStatus = tcpClient.BeginConnect(IP, AlternativePort, networkStatusTelentCallback, tcpClient);
                    var successNetworkStatus = arNetworkStatus.AsyncWaitHandle.WaitOne(TimeOut * 1000);
                    if (!successNetworkStatus)
                    {
                        RaiseTelnetCompletedEvent(false, false);
                    }
                }

            }
            catch (Exception ex)
            {
                RaiseTelnetCompletedEvent(false, false, ex);
            }
        }

        private void NetworkStatusTelentResult(IAsyncResult asyncResult)
        {
            TcpClient oTcpClient = asyncResult.AsyncState as TcpClient;
            try
            {
                oTcpClient.EndConnect(asyncResult);
                if (oTcpClient.Connected)
                {
                    RaiseTelnetCompletedEvent(false, true);
                }
                else
                {
                    throw new Exception(AlternativePort + " not listening.");
                }
            }
            catch (Exception ex)
            {
                RaiseTelnetCompletedEvent(false, false, ex);
            }
        }

        private void CommunicationStatusTelentResult(IAsyncResult asyncResult)
        {
            TcpClient oTcpClient = asyncResult.AsyncState as TcpClient;
            try
            {
                oTcpClient.EndConnect(asyncResult);
                bool tcpResult = oTcpClient.Connected;
                if (tcpResult)
                {
                    RaiseTelnetCompletedEvent(true, true);
                }
                else
                {
                    throw new Exception(Port + " not listening.");
                }
            }
            catch (Exception ex)
            {
                try
                {
                    var networkStatusTelentCallback = new AsyncCallback(NetworkStatusTelentResult);
                    var arNetworkStatus = tcpClient.BeginConnect(IP, AlternativePort, networkStatusTelentCallback, tcpClient);
                    var successNetworkStatus = arNetworkStatus.AsyncWaitHandle.WaitOne(TimeOut * 1000);
                    if (!successNetworkStatus)
                    {
                        RaiseTelnetCompletedEvent(false, false, ex);
                    }
                }
                catch
                {
                    RaiseTelnetCompletedEvent(false, false, ex);
                }
            }
        }
    }
}