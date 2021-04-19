using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Services_Ejer9_NumberGame
{
    class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any,31416);
            Socket socketServe = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socketServe.Bind(ipEndPoint);
                socketServe.Listen(3);
                
                //Cuando aceptemos a un usuario lo metemos en un hilo
            }
            catch (SocketException e) when (e.ErrorCode == (int)SocketError.AddressAlreadyInUse)
            {
                Console.WriteLine("Port already in use");
            }
        }
    }
}
