using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MomCommon.QueueUtils;
using System.Threading;
using MomCommon.ModelosDeIntegracao;

namespace MomCommon
{
    public class GerenciadorDeFilas
    {
        private List<Queue> _filas;
        private Thread _threadDeMonitoramento;
        private List<Porta> _portasUsadas;
        private int _portaDeInicio;

        public GerenciadorDeFilas(int portaDeInicioDefilas)
        {
            this._portaDeInicio = portaDeInicioDefilas;
            this._filas = new List<Queue>();
            this._portasUsadas = new List<Porta>();
        }

        public void IniciarMonitorDeFilas()
        {
            this._threadDeMonitoramento = new Thread(new ThreadStart(MonitorarFilas));
            this._threadDeMonitoramento.Start();
        }

        public void PararMonitorDeFilas()
        {
            if (this._threadDeMonitoramento != null && this._threadDeMonitoramento.IsAlive)
            {
                this._threadDeMonitoramento.Abort();
            }
        }

        private void MonitorarFilas()
        {
            while (true)
            {
                lock (this._filas)
                {
                    this._filas.ForEach(x =>
                    {

                        if (!x.ConexaoAtiva())
                        {
                            x.EncerrarFila();
                            x.IniciarFila();
                        }
                    });
                }
                Thread.Sleep(30000);
            }
        }

        public int InicieNovaFila(Subscribe subscribe)
        {
            if (!ExisteSubscriberComONome(subscribe.nome))
            {
                lock (this._filas)
                {
                    int tentativas = 1;
                    while (tentativas <= 3)
                    {
                        try
                        {
                            var queue = new Queue(subscribe, ObtenhaProximaPortaDisponivel());
                            if (queue.IniciarFila())
                            {
                                this._filas.Add(queue);
                                return queue.ObtenhaPortaUtilizada();
                            }
                            else
                            {
                                throw new IlegalSubscribeException();
                            }
                        }
                        catch
                        {
                            if (tentativas >= 3)
                            {
                                throw;
                            }
                        }
                        tentativas++;
                    }
                    return 0;
                }
            }
            else
            {
                lock (this._filas)
                {
                    return this._filas.Where(x => x.ObtenhaSubscribe().nome == subscribe.nome).Select(x => x.ObtenhaPortaUtilizada()).FirstOrDefault();
                }
            }
        }

        public void EncerrarTodasAsFilas()
        {
            lock (this._filas)
            {
                this._filas.RemoveAll(x =>
                {
                    x.EncerrarFila();
                    return true;
                });
            }
            lock (this._portasUsadas)
            {
                this._portasUsadas.ForEach(x => x.portaEmUso = false);
            }
        }

        public Queue ObtenhaFilaPeloNome(string nomeDaFila)
        {
            lock (this._filas)
            {
                return this._filas.Find(x => x.ObtenhaSubscribe().nome.Equals(nomeDaFila));
            }
        }

        private bool ExisteSubscriberComONome(string nome)
        {
            lock (this._filas)
            {
                return this._filas.FindAll(x => x.ObtenhaSubscribe().nome.Equals(nome)).Count() > 0;
            }
        }

        private int ObtenhaProximaPortaDisponivel()
        {
            lock (this._portasUsadas)
            {
                var listaDePortasDiponiveis = this._portasUsadas.FindAll(x => !x.portaEmUso);
                if (listaDePortasDiponiveis.Count > 0)
                {
                    var porta = listaDePortasDiponiveis.First();
                    porta.portaEmUso = true;
                    return porta.numeroDaPorta;
                }
                else
                {
                    var porta = new Porta();
                    if (this._portasUsadas.Count > 0)
                    {
                        porta.numeroDaPorta = this._portasUsadas.Max(x => x.numeroDaPorta) + 1;
                    }
                    else
                    {
                        porta.numeroDaPorta = this._portaDeInicio;
                    }
                    porta.portaEmUso = true;
                    this._portasUsadas.Add(porta);

                    return porta.numeroDaPorta;
                }
            }
        }

    }

    class Porta
    {
        public int numeroDaPorta;

        public bool portaEmUso;

    }
}
