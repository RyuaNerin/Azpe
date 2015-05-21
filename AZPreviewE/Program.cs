using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace AZPreviewE
{
	static class Program
	{
		public const string TagName = "v0.1.2";
		public const string lpClassName = "AZPreview_Window";
		public const string ProgramName = "AZPreview-E";

		public static string Arg;
		public static string ExePath;
		public static string CacheDir;

		public static IntPtr WP = new IntPtr(26996);

		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length == 0)
				return;

			var ptrWnd = WinApi.FindWindow(Program.lpClassName, null);
			if (ptrWnd != IntPtr.Zero)
			{
				var buff = Encoding.UTF8.GetBytes(args[0]);

				WinApi.COPYDATASTRUCT st = new WinApi.COPYDATASTRUCT();
				st.dwData = Program.WP;
				st.cbData = buff.Length;
				st.lpData = Marshal.AllocHGlobal(buff.Length);

				Marshal.Copy(buff, 0, st.lpData, buff.Length);

				WinApi.SendMessage(ptrWnd, WinApi.WM_COPYDATA, Program.WP, ref st);

				Marshal.FreeHGlobal(st.lpData);

				return;
			}

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			if (args[0] == "exit")
				return;

			Program.Arg			= args[0];
			Program.ExePath		= Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
			Program.CacheDir	= Path.Combine(Program.ExePath, "Cache");

			var form = new frmMain();

			Settings.Load(form);
			Application.Run(form);
		}
	}
}
