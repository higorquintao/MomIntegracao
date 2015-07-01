using MomCommon.ModelosDeIntegracao;
using Newtonsoft.Json;
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
            this._gerenciadorDefilas = gerenciadorDeFilas;
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

        public void PararServidor()
        {
            if (this._servidorIniciado)
            {
                Console.WriteLine("Parando servidor de filas.");
                this._threadPricipal.Abort();
                Console.WriteLine("Servidor de filas parado com sucesso.");
                this._servidorIniciado = false;
            }
        }

        private void RunThread()
        {
            TcpListener servidor = null;
            TcpClient cliente = null;
            try
            {
                IPAddress ip = IPAddress.Parse("127.0.0.1");
                servidor = new TcpListener(ip, this._portaDeInicio);
                servidor.Start();
                while (true)
                {
                    cliente = servidor.AcceptTcpClient();
                    NetworkStream networkStream = cliente.GetStream();
                    try
                    {
                        var subscribe = InterpretarMensagemRecebida(networkStream.ObtenhaRespostaPorString());
                        int porta = 0;
                        lock (this._gerenciadorDefilas)
                        {
                            porta = this._gerenciadorDefilas.InicieNovaFila(subscribe);
                        }
                        networkStream.EnviarMensagemViaJson(new RespostaSubscribe() { sucesso = true, porta = porta, mensagem = "Fila criada com sucesso!" });
                    }
                    catch (ModeloDeIntegracaoIlegalException)
                    {
                        networkStream.EnviarMensagemViaJson(new RespostaSubscribe() { sucesso = false, porta = 0, mensagem = "Não foi possível iniciar uma nova fila, verifique se os dados envidos estão dentro padrão." });
                    }catch(IlegalSubscribeException e)
                    {
                        networkStream.EnviarMensagemViaJson(new RespostaSubscribe() { sucesso = false, porta = 0, mensagem = "Não foi possível iniciar uma nova fila, verifique se os dados envidos estão dentro padrão. Dados inválidos ou fila já existe." });
                    }
                    catch (Exception e)
                    {
                        networkStream.EnviarMensagemViaJson(new RespostaSubscribe() { sucesso = false, porta = 0, mensagem = "Ocorreu um erro inesperado ao inicar fila. " + e.Message });
                        throw new QueueException(string.Format("Erro desconhecido ao inciar fila. {0} {1} {2}", e, e.Message, e.StackTrace));
                    }
                    cliente.Close();
                }
            }
            catch(QueueException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("Ocorreu um erro ao tentar iniciar o servidor de filas verifique se a porta {0} não está em usu por outra aplicação.", _portaDeInicio));
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                if(cliente != null)
                {
                    cliente.Close();
                }
                
                if (servidor != null)
                {
                    servidor.Stop();
                }
                RunThread();
            }
        }

        private Subscribe InterpretarMensagemRecebida(string mensagem)
        {
            try
            {
                return JsonConvert.DeserializeObject<Subscribe>(mensagem);
            }
            catch
            {
                throw new ModeloDeIntegracaoIlegalException();
            }
        }


    }
}
