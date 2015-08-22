using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ComputerBeacon.Json;
using NM = Azpe.NativeMethods;

namespace Azpe
{
	internal static class Handler
	{
		private class AzWindow : IWin32Window
		{
			public IntPtr Handle
			{
				get
				{
					return Handler.m_azHwnd;
				}
			}
		}

		private delegate IntPtr WndProcD(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		private static List<FrmViewer> m_dic = new List<FrmViewer>();

		private static IntPtr	m_winHwnd;
		private static WndProcD	m_winProc;

		private static IntPtr	m_azHwnd;
		private static AzWindow	m_az;
		private static Process	m_azProcess;

		public static void Destroy()
		{
			if (Handler.m_winHwnd != IntPtr.Zero)
			{
				NativeMethods.DestroyWindow(Handler.m_winHwnd);
				Handler.m_winHwnd = IntPtr.Zero;
			}
		}

		public static void Init(string arg)
		{
			Handler.CreateWindow();
			Handler.FindAzurea();
#if !DEBUG
			Handler.UpdateCheck();
#endif
			try
			{
				Handler.ReceiveMessage(arg);
			}
			catch
			{ }
		}

		private static void CreateWindow()
		{
			Handler.m_winProc		= Handler.WndProc;

			var wndClass			= new NM.WNDCLASS();
			wndClass.lpszClassName	= Program.lpClassName;
			wndClass.lpfnWndProc	= Marshal.GetFunctionPointerForDelegate(m_winProc);

			var resRegister	= NM.RegisterClass(ref wndClass);
			var resError	= Marshal.GetLastWin32Error();

			if (resRegister == 0 && resError != NM.ERROR_CLASS_ALREADY_EXISTS)
				throw new Exception();

			Handler.m_azHwnd = NM.CreateWindowEx(0, Program.lpClassName, String.Empty, 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
		}

		private static void FindAzurea()
		{
			Handler.m_azHwnd = NM.FindWindow("Azurea_TwitterClient", null);
			if (Handler.m_azHwnd == IntPtr.Zero)
				throw new Exception();

			int pid;
			if (NativeMethods.GetWindowThreadProcessId(Handler.m_azHwnd, out pid) == 0)
				throw new Exception();

			Handler.m_az = new AzWindow();

			Handler.m_azProcess = Process.GetProcessById(pid);
			Handler.m_azProcess.EnableRaisingEvents = true;
			Handler.m_azProcess.Exited += (ss, ee) => Application.Exit();
		}

		private static void UpdateCheck()
		{
			try
			{
				JsonObject jo;

				var req = HttpWebRequest.Create("https://api.github.com/repos/RyuaNerin/Azpe/releases/latest") as HttpWebRequest;
				req.UserAgent = Program.UserAgent;
				using (var res = req.GetResponse())
				using (var red = new StreamReader(res.GetResponseStream()))
					jo = new JsonObject(red.ReadToEnd());

				var tagName = (string)jo["tag_name"];

				if (tagName != Program.TagName)
				{
					if (MessageBox.Show(
							Handler.m_az,
							string.Format("새 버전이 업데이트 되었어요!\n{0} -> {1}", Program.TagName, tagName),
							"Azpe",
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Question
							) == DialogResult.Yes)
					{
						Process.Start((string)jo["html_url"]);
						Application.Exit();
					}
				}
			}
			catch
			{ }
		}

		private static IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
		{
			if (msg == NM.WM_COPYDATA && wParam == Program.wParam)
			{
				try
				{
					var data = (NM.COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(NM.COPYDATASTRUCT));

					var buff = new byte[data.cbData];
					Marshal.Copy(data.lpData, buff, 0, data.cbData);

					ReceiveMessage(Encoding.UTF8.GetString(buff));
				}
				catch
				{ }

				return IntPtr.Zero;
			}

			return NativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);
		}

		private static void ReceiveMessage(string str)
		{
			if (string.IsNullOrEmpty(str))
				return;

#if !DEBUG
			if (!str.StartsWith(Program.ScriptKey))
			{
				MessageBox.Show(
					Handler.m_az,
					"호환되지 않는 스크립트에요!\nAzpe 를 다시 설치해주세요!",
					"Azpe",
					MessageBoxButtons.OK,
					MessageBoxIcon.Asterisk);
			
				Process.Start("https://github.com/RyuaNerin/Azpe/releases/");

				Application.Exit();
				return;
			}
#endif

#if DEBUG
			if (str.StartsWith(Program.ScriptKey))
#endif
				str = str.Substring(Program.ScriptKey.Length);

			if (str == "exit")
			{
				Application.Exit();
				return;
			}

			if (str == "init")
				return;

			var ss = str.Split(',');
			var id = long.Parse(ss[0]);
			
			switch (ss[1])
			{
				case "close":
					lock (Handler.m_dic)
					{
						var ind = Handler.m_dic.FindIndex(e => e.TweetId == id);
						if (ind != -1)
						{
							var form = Handler.m_dic[ind];
							form.Close();
							form.Dispose();

							Handler.m_dic.RemoveAt(ind);
						}
					}
					break;

				case "top":
					lock (Handler.m_dic)
					{
						var ind = Handler.m_dic.FindIndex(e => e.TweetId == id);
						if (ind != -1)
						{
							var form = Handler.m_dic[ind];
							form.TopMost = !form.TopMost;
						}
					}
					break;

				case "show":
					lock (Handler.m_dic)
					{
						var ind = Handler.m_dic.FindIndex(e => e.TweetId == id);
						if (ind != -1)
						{
							var form = Handler.m_dic[ind];
							form.Show();
							form.Activate();
						}
						else
						{
							FrmViewer form = FrmViewer.Create(id, str);
							if (form != null)
							{
								Handler.m_dic.Add(form);

								form.FormClosed += Handler.FormClosed;
								form.Show();
								form.Activate();
							}
						}
					}
					break;
			}
		}

		private static void FormClosed(object s, EventArgs e)
		{
			FrmViewer form = s as FrmViewer;

			if (form != null)
			{
				lock (Handler.m_dic)
				{
					form.Dispose();
					Handler.m_dic.Remove(form);

					GC.Collect();
				}
			}
		}

		public static FrmViewer NextWindow(FrmViewer form)
		{
			lock (Handler.m_dic)
			{
				int ind = Handler.m_dic.IndexOf(form);
				ind = ++ind % Handler.m_dic.Count;

				return Handler.m_dic[ind];
			}
		}

		public static void CloseAll()
		{
			lock (Handler.m_dic)
			{
				while (Handler.m_dic.Count > 0)
				{
					var form = Handler.m_dic[0];

					form.FormClosed -= Handler.FormClosed;
					form.Close();
					form.Dispose();

					Handler.m_dic.RemoveAt(0);
				}

				GC.Collect();
			}
		}

		public static void ActivateAzurea()
		{
			try
			{
				var placement = new NativeMethods.WINDOWPLACEMENT();
				placement.length = Marshal.SizeOf(placement);
				if (NativeMethods.GetWindowPlacement(Handler.m_azHwnd, ref placement))
				{
					if (placement.showCmd == NativeMethods.ShowCmds.Minimized)
						NativeMethods.ShowWindow(Handler.m_azHwnd, NativeMethods.SW_RESTORE);

					NativeMethods.SetForegroundWindow(Handler.m_azHwnd);
				}
			}
			catch
			{ }
		}
	}
}
