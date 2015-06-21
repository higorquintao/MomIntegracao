using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MomCommon.QueueUtils
{
    public class Queue
    {

        private Thread _threadPrincipal;
        private Subscribe _subscribe;
        private LinkedList<Mensagem> _listaDeMensagens;
        private TcpListener _servidor;
        private TcpClient _cliente;
        private bool _estadoDaConexao;

        private bool _existeNovaMensagem
        {
            get
            {
                return _listaDeMensagens.Where(x => !x.jaConsumida).Count() > 0;
            }
        }


        public Queue(Subscribe subscribe)
        {
            this._subscribe = subscribe;
            this._listaDeMensagens = new LinkedList<Mensagem>();
            this._threadPrincipal = new Thread(new ThreadStart(Run));
            this._threadPrincipal.Start();
        }

        private void Run()
        {
            try
            {
                var ip = IPAddress.Parse("127.0.0.1");
                _servidor = new TcpListener(ip, this._subscribe.porta);
                _servidor.Start();

                byte[] buffer = new byte[1024]; ;
                while (true)
                {
                    _estadoDaConexao = true;

                    _cliente = _servidor.AcceptTcpClient();
                    while (!_existeNovaMensagem)
                    {
                        Thread.Sleep(5000);
                    }

                    var networkStream = _cliente.GetStream();

                    byte[] mensagem = Encoding.UTF8.GetBytes(this.ObtenhaProximaMensagem());

                    networkStream.Write(mensagem, 0, mensagem.Length);

                    _cliente.Close();
                }
            }
            catch
            {

            }
            finally
            {
                if (_servidor != null)
                {
                    _servidor.Stop();
                }
                _estadoDaConexao = false;
                Run();
            }
        }

        public void PublicarMensagem(string mensagem)
        {
            if (mensagem != null)
            {
                this._listaDeMensagens.AddLast(new Mensagem()
                {
                    mensagem = mensagem,
                    jaConsumida = false
                });
            }
        }

        public Subscribe ObtenhaSubscribe()
        {
            return this._subscribe;
        }

        private string ObtenhaProximaMensagem()
        {
            var mensagem = this._listaDeMensagens.First(x => !x.jaConsumida);
            mensagem.jaConsumida = true;
            return mensagem.mensagem;
        }

        public bool ConexaoAtiva()
        {
            return _servidor != null && _cliente != null && _cliente.Connected;
        }

        public void EncerrarFila()
        {
            if (this._cliente != null)
            {
                this._cliente.Close();
            }
            if (this._servidor != null)
            {
                this._servidor.Stop();
            }
            this._threadPrincipal.Abort();
        }
    }
}
