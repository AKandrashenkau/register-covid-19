using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace REGOVID
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1
            {
                StartPosition = (FormStartPosition)1
            });
        }
    }
}
