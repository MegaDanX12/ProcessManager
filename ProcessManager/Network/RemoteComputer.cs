using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ProcessManager.Network
{
    public class RemoteComputer
    {
        /// <summary>
        /// Istanza di <see cref="TcpClient"/> che rappresenta il computer connesso.
        /// </summary>
        private readonly TcpClient Client;

        /// <summary>
        /// Flusso usato per la comunicazione tra client e server.
        /// </summary>
        private readonly NegotiateStream Stream;

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="RemoteComputer"/>.
        /// </summary>
        /// <param name="Client">Istanza di <see cref="TcpClient"/> che rappresenta il computer connesso.</param>
        /// <param name="Stream">Flusso usato per la comunicazione tra client e server.</param>
        public RemoteComputer(TcpClient Client, NegotiateStream Stream)
        {
            this.Client = Client;
            this.Stream = Stream;
        }
    }
}