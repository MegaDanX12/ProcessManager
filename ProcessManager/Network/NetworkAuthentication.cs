using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace ProcessManager.Network
{
    public static class NetworkAuthentication
    {

        public static void ConnectToRemoteComputer(string RemoteAddress, int Port)
        {
            bool IsValidIP = IPAddress.TryParse(RemoteAddress, out IPAddress Address);
            if (IsValidIP)
            {
                if (Port is > 0 and < 65535)
                {
                    TcpClient Client = new();
                    Client.Connect(Address, Port);
                    NegotiateStream Stream = Authenticate(Client);
                    if (Stream is not null)
                    {

                    }
                }
            }
        }


        private static NegotiateStream Authenticate(TcpClient Client)
        {
            NegotiateStream Stream = new(Client.GetStream(), false);
            try
            {
                Stream.AuthenticateAsClient();
                
                return Stream;
            }
            catch (Exception ex) when (ex is AuthenticationException or InvalidCredentialException)
            {
                LogEntry Entry = NativeHelpers.BuildLogEntryForWarning("Tentativo di connessione a un computer remoto fallito", EventAction.RemoteConnection);
                Logger.WriteEntry(Entry);
                return null;
            }
        }
    }
}