using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Services_Ejer9_NumberGame
{
    class Player
    {
        public int Number { set; get; }
        public Socket SocketPlayer { set; get; }
       
        //public string Name { set; get; }

        public Player(int number, Socket socketPlayer)
        {
            this.Number = number;
            this.SocketPlayer = socketPlayer;
        }
    }
}
