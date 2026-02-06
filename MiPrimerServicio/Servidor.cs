using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public bool ServerRunning { get; set; } = true;
        private Socket s;
        private string ruta1 = Environment.ExpandEnvironmentVariables("%programdata%") + "\\puerto.txt";
        private string ruta2 = Environment.ExpandEnvironmentVariables("%programdata%") + "\\log.txt";
        private int Port = 31416;
        public void InitServer()
        {
            int puertoConfig = Leer();
            int puertoFinal = -1;

            if (comprobarPuerto(puertoConfig))
            {
                puertoFinal = puertoConfig;
            }
            else if (comprobarPuerto(Port))
            {
                puertoFinal = Port;
            }
            else
            {
                ServerRunning = false;
            }

            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ie = new IPEndPoint(IPAddress.Any, puertoFinal);
            s.Bind(ie);
            s.Listen(10);

            while (ServerRunning)
            {
                try
                {
                    Socket cliente = s.Accept();
                    Thread hiloCliente = new Thread(() => ClientDispatcher(cliente));
                    hiloCliente.IsBackground = true;
                    hiloCliente.Start();
                }
                catch
                {
                    ServerRunning = false;
                }
            }
            s.Close();
        }

        private void ClientDispatcher(Socket cliente)
        {
            using (cliente)
            {
                IPEndPoint ieCliente = (IPEndPoint)cliente.RemoteEndPoint;

                using (NetworkStream ns = new NetworkStream(cliente))
                using (StreamReader sr = new StreamReader(ns, Encoding.UTF8))
                using (StreamWriter sw = new StreamWriter(ns, Encoding.UTF8))
                {
                    sw.AutoFlush = true;
                    string mensaje = sr.ReadLine();
                    if (mensaje is null)
                    {
                        ServerRunning = false;
                    }
                    Directory.CreateDirectory(Path.GetDirectoryName(ruta2));
                    string linea = $"[{DateTime.Now:yyyy/MM/dd HH:mm:ss}] {mensaje}";
                    File.AppendAllText(ruta2, linea + Environment.NewLine);

                    switch (mensaje.ToLower())
                    {
                        case "time":
                            sw.WriteLine(DateTime.Now.ToLongTimeString());
                            break;

                        case "date":
                            sw.WriteLine(DateTime.Now.ToLongDateString());
                            break;

                        case "all":
                            sw.WriteLine(DateTime.Now.ToString());
                            break;

                        default:
                            sw.WriteLine("Comando no válido");
                            break;
                    }
                }
            }
        }

        private int Leer()
        {
            try
            {
                if (File.Exists(ruta1))
                {
                    return int.Parse(File.ReadAllText(ruta1));
                }
            }
            catch (Exception e)
            {

            }
            return Port;
        }

        private bool comprobarPuerto(int puerto)
        {
            try
            {
                using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    s.Bind(new IPEndPoint(IPAddress.Any, puerto));
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }

}
