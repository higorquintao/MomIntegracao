using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomCommon.QueueUtils
{
    public class Subscribe
    {

        public string nome { get; set; }

        public int porta { get; set; }


        public override bool Equals(object obj)
        {
            if (!(obj is Subscribe))
            {
                return false;
            }

            var subscribe = obj as Subscribe;

            return this.nome == subscribe.nome && this.porta == subscribe.porta;
        }
    }
}
