using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace MomCommon.Utils
{
    public static class StreamExtensions
    {
        public static string ObtenhaRespostaPorString(this NetworkStream stream)
        {
            int ultimoByteLido;
            var dadosRecebidos = new List<byte>();
            byte[] buffer = new byte[128];
            do
            {
                ultimoByteLido = stream.Read(buffer, 0, buffer.Length);
                if (ultimoByteLido == buffer.Length)
                {
                    dadosRecebidos.AddRange(buffer);
                }
                else
                {
                    dadosRecebidos.AddRange(buffer.Take(ultimoByteLido));
                }
            } while (stream.DataAvailable);

            return Encoding.UTF8.GetString(dadosRecebidos.ToArray());
        }

        public static T ObtenhaRespostaPorObjeto<T>(this NetworkStream stream)
        {
            return JsonConvert.DeserializeObject<T>(stream.ObtenhaRespostaPorString());
        }

        public static void EnviarMensagemViaJson(this NetworkStream stream, object obj)
        {
            var mensagemDeRetorno = JsonConvert.SerializeObject(obj);
            stream.EnviarMensagem(mensagemDeRetorno);
        }

        public static void EnviarMensagem(this NetworkStream stream, string mensagem)
        {
            byte[] bytesDeEnvio = Encoding.UTF8.GetBytes(mensagem);
            stream.Write(bytesDeEnvio, 0, bytesDeEnvio.Length);
        }

    }
}
