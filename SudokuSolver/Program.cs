using Kermalis.SudokuSolver.UI;
using System;
using System.Windows.Forms;

namespace Kermalis.SudokuSolver
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}
