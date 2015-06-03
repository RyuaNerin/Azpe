using System;
using System.IO;
using System.Windows.Forms;

namespace Azpe
{
	static class Program
	{
		public const string UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:38.0) Gecko/20100101 Firefox/38.0";

		public const string TagName = "v1.0.2";
		public const string ScriptKey = "a100";

		public const string lpClassName = "azpe_handler";
		public const string ProgramName = "Azpe";
		
		public static string Arg;
		public static string ExePath;
		public static string CacheDir;

		public static IntPtr wParam = new IntPtr(26996);

		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			if (args.Length == 0)
			{
				Application.Run(new FrmInstall());
			}
			else
			{
				var ptrWnd = NativeMethods.FindWindow(Program.lpClassName, null);
				if (ptrWnd != IntPtr.Zero)
				{
					NativeMethods.SendData(ptrWnd, args[0]);
					return;
				}

				if (args[0].EndsWith("exit"))
					return;

				Program.Arg			= args[0];
				Program.ExePath		= Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
				Program.CacheDir	= Path.Combine(Program.ExePath, "Cache");

				FrmMain form;

				try
				{
					form = new FrmMain();
				}
				catch
				{
					return;
				}

				Settings.Load(form);
				
				if (Program.Arg.EndsWith("init"))
					Application.Run();
				else
					Application.Run(form);
			}

		}
	}
}
