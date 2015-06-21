using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomCommon.QueueUtils
{
    class QueueException : Exception
    {
        public QueueException(string mensagem) : base(mensagem)
        {
        }
    }
}
