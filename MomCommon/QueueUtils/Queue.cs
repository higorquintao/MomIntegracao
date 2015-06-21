using MomCommon.ModelosDeIntegracao;
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
        private LinkedList<MensagemParaEnvio> _listaDeMensagens;
        private TcpListener _servidor;
        private TcpClient _cliente;
        private bool _estadoDaConexao;
        private int _porta;

        private bool _existeNovaMensagem
        {
            get
            {
                return _listaDeMensagens.Where(x => !x.jaConsumida).Count() > 0;
            }
        }


        public Queue(Subscribe subscribe, int porta)
        {
            this._subscribe = subscribe;
            this._porta = porta;
            this._listaDeMensagens = new LinkedList<MensagemParaEnvio>();
        }

        private void Run()
        {
            try
            {
                var ip = IPAddress.Parse("127.0.0.1");
                _servidor = new TcpListener(ip, this._porta);
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
            catch (Exception e)
            {
                Console.WriteLine(string.Format("Ocorreu um erro ao tentar iniciar a fila do subscriber {0} na porta {1}.", _subscribe.nome, _porta));
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw;
            }
            finally
            {
                if (_servidor != null)
                {
                    _servidor.Stop();
                }
                _estadoDaConexao = false;
            }
        }

        public void PublicarMensagem(string mensagem)
        {
            if (mensagem != null)
            {
                this._listaDeMensagens.AddLast(new MensagemParaEnvio()
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

        public int ObtenhaPortaUtilizada()
        {
            return this._porta;
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

        public bool IniciarFila()
        {
            if (this._threadPrincipal != null)
            {
                this._threadPrincipal = new Thread(new ThreadStart(Run));
                this._threadPrincipal.Start();
                return true;
            }
            return false;
        }

        public void EncerrarFila()
        {
            lock (this._cliente)
            {
                if (this._cliente != null)
                {
                    this._cliente.Close();
                }
            }
            lock (this._servidor)
            {
                if (this._servidor != null)
                {
                    this._servidor.Stop();
                }
            }
            this._threadPrincipal.Abort();
        }
    }
}
