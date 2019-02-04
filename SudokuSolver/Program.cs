using Kermalis.SudokuSolver.UI;
using System;
using System.Windows.Forms;

namespace Kermalis.SudokuSolver
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}
