using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace phat.Exceptions
{
    internal class ConnectException : Exception
    {
        internal new string Message { get; init; }

        internal int SocketErrorCode { get; init; }
        internal ConnectException(int socketErrorCode, string message = "Cannot connect to the remote address!") : base($"({socketErrorCode}) {message}")
        {
            Message = message;
            SocketErrorCode = socketErrorCode;            
        }
    }
}
