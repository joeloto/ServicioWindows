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

        private string origen = "MiPrimerServicio";

        private string ruta1 = Path.Combine(Environment.ExpandEnvironmentVariables("%programdata%"), "MiPrimerServicio", "config.txt");

        private string ruta2 = Path.Combine(Environment.ExpandEnvironmentVariables("%programdata%"), "MiPrimerServicio", "log.txt");

        private int Port = 31416;
        public void InitServer()
        {
            int puertoConfig = Leer();
            int puertoFinal = -1;

            if (CompruebaPuerto(puertoConfig))
            {
                puertoFinal = puertoConfig;
            }
            else if (CompruebaPuerto(Port))
            {
                puertoFinal = Port;
            }
            else
            {
                EscribirEvento("Puerto ocupado. Servicio finalizado.");
                ServerRunning = false;
            }

            EscribirEvento($"Servidor escuchando en el puerto {puertoFinal}");

            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ie = new IPEndPoint(IPAddress.Any, puertoFinal);
            s.Bind(ie);
            s.Listen(10);

            while (ServerRunning)
            {
                try
                {
                    Socket cliente = s.Accept();
                    Thread hiloCliente = new Thread(() => ClienteDispatcher(cliente));
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

        private void ClienteDispatcher(Socket cliente)
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
                    string linea = $"[{DateTime.Now:yyyy/MM/dd HH:mm:ss}-@{ieCliente.Address}:{ieCliente.Port}] {mensaje}";
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
                            EscribirEvento($"Comando no válido recibido: {mensaje}");
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
                EscribirEvento($"Error al leer archivo de configuración: {e.Message}");
            }

            return Port;
        }

        private bool CompruebaPuerto(int puerto)
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

        private void EscribirEvento(string mensaje)
        {
            try
            {
                if (!EventLog.SourceExists(origen))
                {
                    EventLog.CreateEventSource(origen, "Application");
                }
                EventLog.WriteEntry(origen, mensaje);
            }
            catch (Exception e)
            {
                Error(e.Message);
            }
        }

        private void Error(string error)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ruta2));
            string linea = $"[ERROR] [{DateTime.Now:yyyy/MM/dd HH:mm:ss}] {error}";
            File.AppendAllText(ruta2, linea + Environment.NewLine);
        }
    }

}
