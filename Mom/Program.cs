using MomCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mom
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Inciando MOM.");
                Run.Start();
                Console.WriteLine("MOM Iniciado com sucesso.");
                while (true) ;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
