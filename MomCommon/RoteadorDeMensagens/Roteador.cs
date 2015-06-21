using MomCommon.ModelosDeIntegracao;
using MomCommon.QueueUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MomCommon.RoteadorDeMensagens
{
    public class Roteador
    {
        private int _portaDeInicio;
        private bool _servidorIniciado;
        private Thread _threadPricipal;
        private GerenciadorDeFilas _gerenciadorDefilas;

        public Roteador(int portaDeInicio, GerenciadorDeFilas gerenciadorDeFilas)
        {
            this._portaDeInicio = portaDeInicio;
            this._servidorIniciado = false;
            this._gerenciadorDefilas = gerenciadorDeFilas;
            this._threadPricipal = new Thread(new ThreadStart(RunThread));
        }

        public void IniciarRoteador()
        {
            if (!this._servidorIniciado)
            {
                Console.WriteLine("Iniciando roteador de mensagens.");
                this._threadPricipal.Start();
                Console.WriteLine("Roteador de mensagens iniciado com sucesso.");
                this._servidorIniciado = true;
            }
        }

        public void PararRoteador()
        {
            if (this._servidorIniciado)
            {
                Console.WriteLine("Parando roteador de mensagens.");
                this._threadPricipal.Abort();
                Console.WriteLine("Roteador de mensagens parado com sucesso.");
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
                byte[] buffer = new byte[1024];
                while (true)
                {
                    cliente = servidor.AcceptTcpClient();
                    NetworkStream networkStream = cliente.GetStream();

                    int ultimoByteLido;
                    var stringBuffer = new StringBuilder();
                    while ((ultimoByteLido = networkStream.Read(buffer, 0, buffer.Length)) > -1)
                    {
                        stringBuffer.Append(buffer);
                    }
                    try
                    {
                        var mensagem = InterpretarMensagemRecebida(stringBuffer.ToString());
                        lock (this._gerenciadorDefilas)
                        {
                            var queue = this._gerenciadorDefilas.ObtenhaFilaPeloNome(mensagem.nomeFila);
                            if(queue != null)
                            {
                                queue.PublicarMensagem(mensagem.mensagem);

                            }else
                            {
                                var mensagemDeRetorno = JsonConvert.SerializeObject(new RespostaSubscribe() { sucesso = false, porta = 0, mensagem = "Não foi possível publicar sua mensagem, verifique se o nome da fila esta correto." });
                                byte[] bytesDeEnvio = Encoding.UTF8.GetBytes(mensagemDeRetorno);
                                networkStream.Write(bytesDeEnvio, 0, bytesDeEnvio.Length);
                            }
                        }
                    }
                    catch (ModeloDeIntegracaoIlegalException)
                    {
                        var mensagemDeRetorno = JsonConvert.SerializeObject(new RespostaSubscribe() { sucesso = false, porta = 0, mensagem = "Não foi possível iniciar uma nova fila, verifique se os dados envidos estão dentro padrão." });
                        byte[] bytesDeEnvio = Encoding.UTF8.GetBytes(mensagemDeRetorno);
                        networkStream.Write(bytesDeEnvio, 0, bytesDeEnvio.Length);
                        throw new QueueException("Erro ao submeter mensagem, formato da mensagem inválido.");
                    }
                    catch (Exception e)
                    {
                        var mensagemDeRetorno = JsonConvert.SerializeObject(new RespostaSubscribe() { sucesso = false, porta = 0, mensagem = "Ocorreu um erro inesperado ao obter fila. " + e.Message });
                        byte[] bytesDeEnvio = Encoding.UTF8.GetBytes(mensagemDeRetorno);
                        networkStream.Write(bytesDeEnvio, 0, bytesDeEnvio.Length);
                        throw new QueueException(string.Format("Erro desconhecido ao obter mensagem. {0} {1} {2}", e, e.Message, e.StackTrace));
                    }
                    cliente.Close();
                }
            }
            catch (QueueException e)
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
                if (cliente != null)
                {
                    cliente.Close();
                }

                if (servidor != null)
                {
                    servidor.Stop();
                }
                Console.WriteLine("Iniciando novamente em 10 segundo.");
                Thread.Sleep(10000);
                RunThread();
            }
        }

        private Mensagem InterpretarMensagemRecebida(string mensagem)
        {
            try
            {
                return JsonConvert.DeserializeObject<Mensagem>(mensagem);
            }
            catch
            {
                throw new ModeloDeIntegracaoIlegalException();
            }
        }

    }
}
