using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiPrimerServicio
{
    internal class Servidor
    {
        public bool ServerRunning { set; get; } = true;
        public int Port { get; set; } = 135;
        public int[] puertos = { 135, 8888, 31416};
        private Socket s;

        public void InitServer()
        {
            IPEndPoint ie = new IPEndPoint(IPAddress.Any, compruebaPuerto(puertos));
            using (s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                s.Bind(ie);
                s.Listen(10);
                Console.WriteLine($"Servidor iniciado. " +
                $"Escuchando en {ie.Address}:{ie.Port}");
                Console.WriteLine("Comandos disponibles: ");
                Console.WriteLine("time: hora, minutos y segundos");
                Console.WriteLine("date: dia, mes y ano");
                Console.WriteLine("all: fecha y hora");
                Console.WriteLine("close: debe ir acompañado de una password de 6 caracteres");

                while (ServerRunning)
                {
                    try
                    {
                        Socket client = s.Accept();
                        Thread hilo = new Thread(() => ClientDispatcher(client));
                        hilo.Start();
                        hilo.IsBackground = true;

                    }
                    catch (SocketException e)
                    {
                        ServerRunning = false;
                    }
                }
            }
        }

        private void ClientDispatcher(Socket sClient)
        {
            using (sClient)
            {
                IPEndPoint ieClient = (IPEndPoint)sClient.RemoteEndPoint;
                Console.WriteLine($"Cliente conectado:{ieClient.Address} " +
                $"en puerto {ieClient.Port}");
                Encoding codificacion = Console.OutputEncoding;
                using (NetworkStream ns = new NetworkStream(sClient))
                using (StreamReader sr = new StreamReader(ns, codificacion))
                using (StreamWriter sw = new StreamWriter(ns, codificacion))
                {
                    sw.AutoFlush = true;
                    //string welcome = "Elige uno de los comandos";
                    //sw.WriteLine(welcome);
                    string msg = "";
                    //while (msg != null)
                    //{
                    try
                    {
                        msg = sr.ReadLine();
                        if (msg != null)
                        {
                            if (msg.ToLower() == "time")
                            {
                                sw.WriteLine(DateTime.Now.ToLongTimeString());
                            }
                            if (msg.ToLower() == "date")
                            {
                                sw.WriteLine(DateTime.Now.ToLongDateString());
                            }
                            if (msg.ToLower() == "all")
                            {
                                sw.WriteLine(DateTime.Now);
                            }
                            if (msg.StartsWith("close "))
                            {
                                string ruta = $"{Environment.ExpandEnvironmentVariables("%PROGRAMDATA%")}\\password.txt";
                                string pass = msg.Substring(6);
                                if (pass.Length < 6)
                                {
                                    sw.WriteLine("La contraseña debe tener mínimo 6 caracteres");
                                    sw.WriteLine("Prueba de nuevo o utiliza o otro comando");
                                }
                                else
                                {
                                    if (File.Exists(ruta))
                                    {
                                        using (StreamReader sw2 = new StreamReader(ruta))
                                        {
                                            string p = sw2.ReadLine();
                                            if (p == pass)
                                            {
                                                sw.WriteLine("Password válida, se cierra la conexión");
                                                
                                            }
                                        }
                                    }
                                    else
                                    {
                                        sw.Write("La ruta no existe");
                                    }

                                }
                            }

                        }
                    }
                    catch (IOException)
                    {
                        msg = null;
                    }
                }
                Console.WriteLine("Cliente desconectado.\nConexión cerrada");
            }
        }

        private int compruebaPuerto(int[] puertos)
        {
            int puertoCorrecto = 0;
            for (int i = 0; i < puertos.Length; i++)
            {
                IPEndPoint ie;
                using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    try
                    {
                        ie = new IPEndPoint(IPAddress.Any, puertos[i]);
                        s.Bind(ie);
                        puertoCorrecto = puertos[i];
                    }
                    catch (SocketException e) when (e.ErrorCode == (int)SocketError.AddressAlreadyInUse)
                    {

                    }
                    catch (SocketException e)
                    {

                    }
                }
            }
            return puertoCorrecto;

        }
    }
}
