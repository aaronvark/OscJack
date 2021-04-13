// OSC Jack - Open Sound Control plugin for Unity
// https://github.com/keijiro/OscJack

using System;
using System.Net;
using System.Net.Sockets;

namespace OscJack
{
    public sealed class OscClient : IDisposable
    {
        #region Object life cycle

        public OscClient(string destination, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            if (destination == "255.255.255.255")
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

            var dest = new IPEndPoint(IPAddress.Parse(destination), port);
            _socket.Connect(dest);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Packet sender methods

        public void Send(string address)
        {
            _encoder.Clear();
            _encoder.Append(address);
            _encoder.Append(",");
            _socket.Send(_encoder.Buffer, _encoder.Length, SocketFlags.None);
        }

        public void Send( string address, params object[] data) {
            if (data == null) {
                Send(address);
                return;
            }

            _encoder.Clear();
            _encoder.Append(address);

            string format = ",";
            foreach (object o in data) {
                if      (o is string)   format += "s";
                else if (o is int)      format += "i";
                else if (o is float)    format += "f";
            }

            _encoder.Append(format);

            foreach (object o in data) {
                if      (o is string)   _encoder.Append((string)o);
                else if (o is int)      _encoder.Append((int)o);
                else if (o is float)    _encoder.Append((float)o);
            }

            _socket.Send(_encoder.Buffer, _encoder.Length, SocketFlags.None);
        }

        #endregion

        #region IDispose implementation

        bool _disposed;

        void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                if (_socket != null)
                {
                    _socket.Close();
                    _socket = null;
                }

                _encoder = null;
            }
        }

        ~OscClient()
        {
            Dispose(false);
        }

        #endregion

        #region Private variables

        OscPacketEncoder _encoder = new OscPacketEncoder();
        Socket _socket;

        #endregion
    }
}
