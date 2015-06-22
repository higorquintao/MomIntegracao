using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MomCommon;
namespace Mom
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            try {
                Console.WriteLine("Inciando MOM.");
                Run.Start();
                Console.WriteLine("MOM Iniciado com sucesso.");
                while (true);
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
