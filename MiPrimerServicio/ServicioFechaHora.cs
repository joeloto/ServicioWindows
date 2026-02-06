using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiPrimerServicio
{
    public partial class ServicioFechaHora : ServiceBase
    {
        private Servidor servidor;
        private Thread hiloServidor;

        public ServicioFechaHora()
        {
            InitializeComponent();
            CanPauseAndContinue = false;
            CanStop = true;
            AutoLog = false;
        }

        protected override void OnStart(string[] args)
        {
            servidor = new Servidor();
            hiloServidor = new Thread(servidor.InitServer);
            hiloServidor.IsBackground = true;
            hiloServidor.Start();
        }

        protected override void OnStop()
        {
            servidor.ServerRunning = false;
        }
    }
}
