using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Subscriber
{
    class Program
    {
        private static RespostaSubscribe respostaSubscriber;
        private static string ipServidor;
        static void Main(string[] args)
        {
            Console.WriteLine("Informe o ip ou dominio do servidor:");
            ipServidor = Console.ReadLine();
            while (true)
            { 
                PrintMenu();
                var itemSelecionado = 0;
                try
                {
                    itemSelecionado = int.Parse(Console.ReadLine());
                }
                catch { }

                switch (itemSelecionado)
                {
                    case 1:
                        Console.Clear();
                        IniciarUmaNovaFila();
                        break;
                    case 2:
                        Console.Clear();
                        ReceberMensagem();
                        break;
                    case 3:
                        Console.Clear();
                        EnviarMensagem();
                        break;
                    default:
                        Console.WriteLine("Opção informada é inválida. Tente novamente");
                        break;
                }
            }
        }

        static void ConectarEmFilaEEscutarMensagens(int porta)
        {
            TcpClient client = new TcpClient();
            client.Connect(ipServidor, porta);
            try
            {
                var stream = client.GetStream();

                stream.EnviarMensagemViaJson(new SolicitacaoDeMensagem() { obterNovaMensagem = true });

                var mensagemRecebida = stream.ObtenhaRespostaPorObjeto<MensagemParaEnvio>();
                if (mensagemRecebida.mensagem != null)
                {
                    Console.WriteLine("Nova mensagem recebida: " + mensagemRecebida.mensagem);
                }
                else
                {
                    Console.WriteLine("Nenhuma mensagem a ser exibida.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro ao obter mensagem, tentando novamente... Erro: " + e.Message);
            }
            finally
            {
                if (!client.Connected)
                {
                    client.Connect("localhost", porta);
                }
            }
        }

        static void PrintMenu()
        {
            Console.WriteLine("Informe uma das opções abaixo:");
            Console.WriteLine("1 - Iniciar uma nova fila");
            Console.WriteLine("2 - receber mensagem");
            Console.WriteLine("3 - Enviar mensagem para alguma fila.");
            Console.WriteLine("....");
        }

        static void IniciarUmaNovaFila()
        {
            Console.WriteLine("Para se inscrever digite o nome da fila:");
            var nomeSubscriber = Console.ReadLine();
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(ipServidor, 3000);
                var stream = client.GetStream();
                stream.EnviarMensagemViaJson(new Subscribe()
                {
                    nome = nomeSubscriber
                });

                var informacoes = stream.ObtenhaRespostaPorObjeto<RespostaSubscribe>();

                client.Close();

                Console.WriteLine(informacoes.mensagem);
                respostaSubscriber = informacoes;
            }
            catch
            {
                Console.WriteLine("Servidor de mensagens indisponível no momento.");
            }
        }

        static void ReceberMensagem()
        {
            if (respostaSubscriber != null)
            {
                ConectarEmFilaEEscutarMensagens(respostaSubscriber.porta);
            }
            else
            {
                Console.WriteLine("Fila não iniciada.");
            }
        }

        static void EnviarMensagem()
        {
            Console.WriteLine("Digite o nome da fila:");
            var nomeSubscriber = Console.ReadLine();

            Console.WriteLine("Digite sua mensagem:");
            var mensagem = Console.ReadLine();

            try
            {
                TcpClient client = new TcpClient();
                client.Connect(ipServidor, 3001);
                var stream = client.GetStream();
                stream.EnviarMensagemViaJson(new Mensagem()
                {
                    mensagem = mensagem,
                    nomeFila = nomeSubscriber
                });

                client.Close();
                Console.WriteLine("Mensagem publicada com sucesso.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro ao enviar mensagem. Erro: " + e.Message);
            }
        }
    }

    class RespostaSubscribe
    {
        public bool sucesso;

        public int porta;

        public string mensagem;
    }

    class Subscribe
    {

        public string nome { get; set; }

        public int timeout { get; set; }

        public int tamanhoMaximoDeMensagem { get; set; }

    }

    class SolicitacaoDeMensagem
    {
        public bool obterNovaMensagem;
    }

    class MensagemParaEnvio
    {
        public string mensagem { get; set; }

        public bool jaConsumida { get; set; }
    }

    class Mensagem
    {
        public string nomeFila;

        public string mensagem;
    }

}