using System;
using System.IO;
using System.Windows.Forms;

namespace Azpe
{
	internal static class Program
	{
		public const string UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:38.0) Gecko/20100101 Firefox/38.0";

		public const string TagName = "v1.1.0";
		public const string ScriptKey = "a110";

		public const string lpClassName = "azpe_handler";
		
		public static string ExePath;

		public static readonly IntPtr wParam = new IntPtr(26996);

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

				Program.ExePath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

				Cache.Init();
				Settings.Init();

				try
				{
					Handler.Init(args.Length > 0 ? args[0] : null);	
				}
				catch
				{
					return;
				}

				Application.Run();
			}
		}
	}
}
