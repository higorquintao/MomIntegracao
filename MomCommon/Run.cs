using MomCommon.QueueUtils;
using MomCommon.RoteadorDeMensagens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomCommon
{
    public class Run
    {
        private static bool _momIniciado = false;
        private static GerenciadorDeFilas _gerenciadorDeFilas;
        private static QueueServer _queueServer;
        private static Roteador _roteadorDeMensagens;

        public static void Start()
        {   
            _gerenciadorDeFilas = new GerenciadorDeFilas(3002);
            _gerenciadorDeFilas.IniciarMonitorDeFilas();
            _queueServer = new QueueServer(3000, _gerenciadorDeFilas);
            _queueServer.IniciarServidor();
            _roteadorDeMensagens = new Roteador(3001, _gerenciadorDeFilas);
            _roteadorDeMensagens.IniciarRoteador();
        }

        public static void Stop()
        {
            _gerenciadorDeFilas.EncerrarTodasAsFilas();
            _gerenciadorDeFilas.PararMonitorDeFilas();
            _queueServer.PararServidor();
            _roteadorDeMensagens.PararRoteador();
        }
    }
}
