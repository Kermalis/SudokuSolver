using System;
using System.Windows.Forms;

namespace Kermalis.SudokuSolver.UI;

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
