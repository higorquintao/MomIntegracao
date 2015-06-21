using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MomCommon.QueueUtils
{
    class QueueServer
    {

        private int _portaDeInicio;
        private bool _servidorIniciado;
        private Thread _threadPricipal;
        private GerenciadorDeFilas _gerenciadorDefilas;

        public QueueServer(int portaDeInicio, GerenciadorDeFilas gerenciadorDeFilas)
        {
            this._portaDeInicio = portaDeInicio;
            this._servidorIniciado = false;
            this._threadPricipal = new Thread(new ThreadStart(RunThread));
        }

        public void IniciarServidor()
        {
            if (!this._servidorIniciado)
            {
                Console.WriteLine("Iniciando servidor de filas.");
                this._threadPricipal.Start();
                Console.WriteLine("Servidor de filas iniciado com sucesso.");
                this._servidorIniciado = true;
            }
        }

        public void RunThread()
        {
            TcpListener servidor = null;
            try
            {
                IPAddress ip = IPAddress.Parse("127.0.0.1");
                servidor = new TcpListener(ip, this._portaDeInicio);
                servidor.Start();

                byte[] buffer = new byte[1024];
                string data;
                while (true)
                {
                    TcpClient cliente = servidor.AcceptTcpClient();
                    data = null;
                    NetworkStream networkStream = cliente.GetStream();

                    int ultimoByteLido;

                    while((ultimoByteLido = networkStream.Read(buffer, 0, buffer.Length)) > -1)
                    {

                    }
                }
            }
            catch
            {

            }
            finally
            {
                if (servidor != null)
                {
                    servidor.Stop();
                }
                RunThread();
            }
        }

        public void InterpretarMensagemRecebida(string mensagem)
        {
            
        }
        

    }
}
