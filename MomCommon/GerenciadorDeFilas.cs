using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MomCommon.QueueUtils;
using System.Threading;

namespace MomCommon
{
    public class GerenciadorDeFilas
    {
        private List<Queue> _filas;
        private Thread _threadDeMonitoramento;

        public GerenciadorDeFilas()
        {
            this._filas = new List<Queue>();
            this._threadDeMonitoramento = new Thread(new ThreadStart(MonitorarFilas));
            this._threadDeMonitoramento.Start();
        }

        public void MonitorarFilas()
        {
            while (true)
            {
                lock (this._filas)
                {
                    this._filas.FindAll(x => !x.ConexaoAtiva()).ForEach(x => x.EncerrarFila());
                }
                Thread.Sleep(30000);
            }
        }

        public void CadastreNovaFila(Queue queue)
        {
            lock (this._filas)
            {
                this._filas.Add(queue);
            }
        }

        public Queue ObtenhaFila(Subscribe subscribe)
        {
            lock (this._filas)
            {
                return this._filas.Find(x => x.ObtenhaSubscribe().Equals(subscribe));
            }
        }
    }
}
