using MomCommon.ModelosDeIntegracao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MomCommon.Utils;

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

                byte[] buffer = new byte[1024];
                _cliente = _servidor.AcceptTcpClient();
                _estadoDaConexao = true;
                while (true)
                {
                    var networkStream = _cliente.GetStream();
                    var solicitacaoDeMensagem = networkStream.ObtenhaRespostaPorObjeto<SolicitacaoDeMensagem>();

                    if (solicitacaoDeMensagem.obterNovaMensagem)
                    {
                        networkStream.EnviarMensagemViaJson(this.ObtenhaProximaMensagem());
                    }
                }
            }
            catch (Exception e)
            {
                if (_cliente == null)
                {
                    Console.WriteLine(string.Format("Ocorreu um erro ao tentar iniciar a fila do subscriber {0} na porta {1}.", _subscribe.nome, _porta));
                }else
                {
                    Console.WriteLine(string.Format("Subscriber {0} na porta {1} se desconectou.", _subscribe.nome, _porta));
                }
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                if (_cliente != null)
                {
                    _cliente.Close();
                }
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

        private MensagemParaEnvio ObtenhaProximaMensagem()
        {
            var mensagem = this._listaDeMensagens.Where(x => !x.jaConsumida).FirstOrDefault();
            if (mensagem != null)
            {
                mensagem.jaConsumida = true;
                return mensagem;
            }
            else
            {
                return new MensagemParaEnvio();
            }
        }

        public bool ConexaoAtiva()
        {
            return _servidor != null;
        }

        public bool IniciarFila()
        {
            if (this._threadPrincipal == null)
            {
                this._threadPrincipal = new Thread(new ThreadStart(Run));
                this._threadPrincipal.Start();
                return true;
            }
            return false;
        }

        public void EncerrarFila()
        {
            if (this._cliente != null)
            {
                lock (this._cliente)
                {
                    this._cliente.Close();
                }
            }
            if (this._servidor != null)
            {
                lock (this._servidor)
                {
                    this._servidor.Stop();
                }
            }
            if (this._threadPrincipal != null)
            {
                this._threadPrincipal.Abort();
            }
        }
    }
}
