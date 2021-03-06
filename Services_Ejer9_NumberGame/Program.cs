using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/*Juego número más alto en red. Al servidor se conectan al menos dos clientes (puede haber más) y les da un 
número aleatorio entre 1 y 20. Cuando todos los clientes estén conectados (Hazlo por tiempo. Que los 
clientes vean el tiempo que queda para empezar) el servidor comprueba quién es el ganador e informa de 
ello a los clientes. Debe indicar a los que pierde el número ganador además del número que sacaron.*/

namespace Services_Ejer9_NumberGame
{
    class Program
    {

        static List<Player> playerList = new List<Player>();
        static Random randomNumber = new Random();
        static bool gameStart = false;
        static int timeToStartGame =20;//descendemos a 0
        static object key = new object();
        
        static int numberWin = 0;
        static bool repeatGame = false;
        static bool timerStart = false;


        static void Main(string[] args)
        {

            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 31416);
            Socket socketServe = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                do
                {
                    socketServe.Bind(ipEndPoint);
                    socketServe.Listen(3);


                   

                    //Cuando aceptemos a un usuario lo metemos en un hilo
                    while (!gameStart)
                    {
                        Socket socketClient = socketServe.Accept();

                        //creamos un hilo para el cliente conectado
                        Thread thread = new Thread(Game);
                        repeatGame = false;
                        thread.Start(socketClient);

                    }
                } while (repeatGame);
            }
            catch (SocketException e) when (e.ErrorCode == (int)SocketError.AddressAlreadyInUse)
            {
                Console.WriteLine("Port already in use");
            }

        }

        static void Game(object socket)
        {
            Socket socketPlayer = (Socket)socket;
            lock (key)
            {
                playerList.Add(new Player(randomNumber.Next(1, 21), socketPlayer));
                Console.WriteLine("Coleccion tamaño:"+playerList.Count);
            }

            using (NetworkStream ns = new NetworkStream(socketPlayer))
            using (StreamReader sr = new StreamReader(ns))
            using (StreamWriter sw = new StreamWriter(ns))
            {
                lock (key)
                {
                    //El elemento común es la consola por la que saldrá los mensajes
                    sw.WriteLine("Hello");
                    sw.WriteLine("num usuarios:" + playerList.Count);
                }
                if (playerList.Count >= 2 && !timerStart)
                {

                    //si ya hay dos jugadores lanzamos la cuenta atrás y el timer al llegar a 0 cambia gamestart a true para que no entren más jugadores

                    //El hilo del timer
                    timerStart = true;
                    Thread threadTimer = new Thread(timerGame);
                    threadTimer.Start();
                }


            }
        }

        static void timerGame()
        {

            //cuenta atrás
            while (timeToStartGame > 0)
            {
                Console.WriteLine("Entro aqui");
                //El numero ganador de la partida
                numberWin = randomNumber.Next(1, 21);

                Thread.Sleep(1000);

                //variable común para más de un método
                timeToStartGame--;
                
                //metemos en un lock pues accedemos a la colección
                lock (key)
                {
                    for (int i = 0; i < playerList.Count; i++)
                    {

                        try
                        {
                            using (NetworkStream ns = new NetworkStream(playerList[i].SocketPlayer))
                            using (StreamReader sr = new StreamReader(ns))
                            using (StreamWriter sw = new StreamWriter(ns))
                            {
                                sw.WriteLine("Time to start: {0}", timeToStartGame);
                            }
                        }
                        catch (Exception)
                        {
                            //De esta forma si se desconecta alguien lo quita de la colección y sigue con el resto de jugadores
                            Console.WriteLine("Error");
                            playerList.RemoveAt(i);
                        }
                    }
                }
                
            }

            //comunicamos el resultado
            lock (key)
            {
                for (int i = 0; i < playerList.Count; i++)
                {
                    try
                    {
                        using (NetworkStream ns = new NetworkStream(playerList[i].SocketPlayer))
                        using (StreamReader sr = new StreamReader(ns))
                        using (StreamWriter sw = new StreamWriter(ns))
                        {
                            //pasar el numero aleatorio
                            sw.WriteLine("numero:{0}", playerList[i].Number);

                            //comparar
                            if (playerList[i].Number == numberWin)
                            {
                                sw.WriteLine("You win");
                            }
                            else
                            {
                                sw.WriteLine("You lose the correct numbert is:{0}", numberWin);
                            }

                        }
                    }

                    catch (Exception)
                    {
                        //De esta forma si se desconecta alguien lo quita de la colección y sigue con el resto de jugadores
                        Console.WriteLine("Error");
                        playerList.RemoveAt(i);
                    }
                }
            }


            //Les quitamos la conexión con el servidor
            lock (key)
            {
                for (int i = 0; i < playerList.Count; i++)
                {
                    try
                    {
                        playerList[i].SocketPlayer.Close();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error");
                        playerList.RemoveAt(i);
                    }
                }
            }
           

            //Reiniciamos
            lock (key)
            {
                Console.WriteLine("Fin juego");
                playerList.RemoveRange(0, playerList.Count); //creo que vaciamos la colección
                repeatGame = true;
                timeToStartGame = 5;
                timerStart = false;
                //cerrar el hilo
            }

            Console.WriteLine(playerList.Count);
        }
    }
}
