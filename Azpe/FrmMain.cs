using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Azpe.Viewer;
using System.Linq;
using ComputerBeacon.Json;
using System.Collections.Generic;

namespace Azpe
{
	public partial class FrmMain : Form, IMessageFilter
	{
		private CustomWnd	m_customWnd;
		private Process		m_azurea;

		private AzpViewer	m_viewer;

		public FrmMain()
		{
			InitializeComponent();

			IntPtr hwnd = NativeMethods.FindWindow("Azurea_TwitterClient", null);
			if (hwnd == IntPtr.Zero)
				throw new Exception();

			int pid;
			if (NativeMethods.GetWindowThreadProcessId(hwnd, out pid) == 0)
				throw new Exception();

			this.m_azurea = Process.GetProcessById(pid);
			this.m_azurea.EnableRaisingEvents = true;
			this.m_azurea.Exited += (ss, ee) => Application.Exit();

			this.m_customWnd = new CustomWnd(Program.lpClassName, CopyDataProc);

			this.m_viewer = new AzpViewer(this);

			this.Text = string.Format("[ 0 / 0 ] {0}", Program.ProgramName);
			
			if (Program.Arg.EndsWith("init"))
				this.Visible = false;

			Application.AddMessageFilter(this);
		}

		private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing)
			{
				this.Hide();
				e.Cancel = true;
			}
			else
			{
				this.m_customWnd.Dispose();
				this.ntf.Visible = false;
			}
		}

		private void frmMain_Shown(object sender, EventArgs e)
		{
			if (!Program.Arg.EndsWith("init"))
			{
				this.Show();
				this.ReceiveMessage(Program.Arg);
			}

#if !DEBUG
			new Task(() => UpdateCheck()).Start();
#endif
		}

		private void UpdateCheck()
		{
			if ((DateTime.UtcNow - Settings.NewChecked).TotalDays < 1)
				return;

			try
			{
				JsonObject jo;

				var req = HttpWebRequest.Create("https://api.github.com/repos/RyuaNerin/Azpe/releases/latest") as HttpWebRequest;
				req.UserAgent = Program.UserAgent;
				using (var res = req.GetResponse())
				using (var red = new StreamReader(res.GetResponseStream()))
					jo = new JsonObject(red.ReadToEnd());

				if ((string)jo["tag_name"] != Program.TagName)
				{
					this.Invoke(
						new Action(
							() =>
							{
								try
								{
									if (MessageBox.Show(
											this,
											"새 버전이 업데이트 되었어요!",
											Program.ProgramName,
											MessageBoxButtons.YesNo,
											MessageBoxIcon.Question
											) == DialogResult.Yes)
									{
										Process.Start((string)jo["html_url"]);
										Application.Exit();
									}
									else
									{
										Settings.NewChecked = DateTime.UtcNow;
										Settings.Save();
									}
								}
								catch
								{
								}
							}
						)
					);
				}
			}
			catch
			{
			}
		}
		
		private IntPtr CopyDataProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == NativeMethods.WM_COPYDATA && wParam == Program.wParam)
			{
				var data = (NativeMethods.COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(NativeMethods.COPYDATASTRUCT));

				var buff = new byte[data.cbData];
				Marshal.Copy(data.lpData, buff, 0, data.cbData);

				this.ReceiveMessage(Encoding.UTF8.GetString(buff));

				handled = true;
			}

			return IntPtr.Zero;
		}
		
		private void ReceiveMessage(string str)
		{
#if !DEBUG
			if (!str.StartsWith(Program.ScriptKey))
			{
				MessageBox.Show(
					this,
					"호환되지 않는 스크립트에요!\n다시 설치해주세요!",
					Program.ProgramName,
					MessageBoxButtons.OK,
					MessageBoxIcon.Asterisk);
			
				Process.Start("http://ryuanerin.github.io/Azpe");

				Application.Exit();
				return;
			}
#endif

#if DEBUG
			if (str.StartsWith(Program.ScriptKey))
#endif
				str = str.Remove(0, Program.ScriptKey.Length);

			if (str == "hide")
				this.Hide();

			else if (str == "top")
				this.TopMost = !this.TopMost;

			else if (str == "focus")
			{
				this.Show();
				NativeMethods.FocusWindow(this.Handle);
				this.Activate();
			}
			else if (str == "left")
				this.m_viewer.CurrentIndex--;

			else if (str == "right")
				this.m_viewer.CurrentIndex++;

			else if (str.Length > 0)
			{
				this.Show();
				NativeMethods.FocusWindow(this.Handle);
				this.Activate();

				this.m_viewer.AddUrl(str);
			}
		}

		private void CalcScreen()
		{
			this.m_xMin = this.m_yMin = int.MaxValue;
			this.m_xMax = this.m_yMax = int.MinValue;

			foreach (Screen screen in Screen.AllScreens)
			{
				var bounds = screen.Bounds;

				this.m_xMin = Math.Min(this.m_xMin, bounds.Left);
				this.m_yMin = Math.Min(this.m_yMin, bounds.Y);

				this.m_xMax = Math.Max(this.m_xMax, bounds.Right);
				this.m_yMax = Math.Max(this.m_yMax, bounds.Bottom);
			}
		}

		private const int WM_KEYDOWN = 0x100;
		private const int WM_KEYUP = 0x101;
		private const int WM_SYSKEYDOWN = 0x104;

		private int		m_move;
		private int		m_xMin, m_xMax;
		private int		m_yMin, m_yMax;
		private bool	m_keyRepeat;

		public bool PreFilterMessage(ref Message m)
		{
			if (m.Msg == FrmMain.WM_KEYUP)
				this.m_keyRepeat = false;
			return false;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData)
			{
				case Keys.Left:		this.m_viewer.CurrentIndex--;	return true;
				case Keys.Right:	this.m_viewer.CurrentIndex++;	return true;

				//////////////////////////////////////////////////////////////////////////

				case Keys.Control | Keys.Left:	this.m_viewer.ImageMove(-1, 0);	return true;
				case Keys.Control | Keys.Right:	this.m_viewer.ImageMove(1, 0);	return true;
				case Keys.Control | Keys.Up:	this.m_viewer.ImageMove(0, -1);	return true;
				case Keys.Control | Keys.Down:	this.m_viewer.ImageMove(0, 1);	return true;

				//////////////////////////////////////////////////////////////////////////

				case Keys.Shift | Keys.Left:
					if (msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN)
					{
						if (!this.m_keyRepeat)
						{
							this.m_move = 0;
							this.m_keyRepeat = true;
							this.CalcScreen();
						}

						if (this.m_move < 30) this.m_move++;

						var val = this.Left - this.m_move;
						if (val < this.m_xMin - this.Width + 20)
							val = this.m_xMin - this.Width + 20;
						this.Left = val;
					}
					else
					{
						this.m_keyRepeat = false;
						Settings.Save();
					}
					break;

				case Keys.Shift | Keys.Right:
					if (msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN)
					{
						if (!this.m_keyRepeat)
						{
							this.m_move = 0;
							this.m_keyRepeat = true;
							this.CalcScreen();
						}

						if (this.m_move < 30) this.m_move++;

						var val = this.Left + this.m_move;
						if (val > this.m_xMax - 20)
							val = this.m_xMax - 20;
						this.Left = val;
					}
					else
					{
						this.m_keyRepeat = false;
						Settings.Save();
					}
					break;

				case Keys.Shift | Keys.Up:
					if (msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN)
					{
						if (!this.m_keyRepeat)
						{
							this.m_move = 0;
							this.m_keyRepeat = true;
							this.CalcScreen();
						}

						if (this.m_move < 30) this.m_move++;

						var val = this.Top - this.m_move;
						if (val < this.m_yMin - this.Height + 20)
							val = this.m_yMin - this.Height + 20;
						this.Top = val;
					}
					else
					{
						this.m_keyRepeat = false;
						Settings.Save();
					}
					break;

				case Keys.Shift | Keys.Down:
					if (msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN)
					{
						if (!this.m_keyRepeat)
						{
							this.m_move = 0;
							this.m_keyRepeat = true;
							this.CalcScreen();
						}

						if (this.m_move < 30) this.m_move++;

						var val  = this.Top + this.m_move;
						if (val > this.m_yMax)
							val = this.m_yMax;
						this.Top = val;
					}
					else
					{
						this.m_keyRepeat = false;
						Settings.Save();
					}
					break;

				//////////////////////////////////////////////////////////////////////////

				case Keys.Alt | Keys.Left:
					if (msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN)
					{
						int val = this.Width - 20;
						if (val < this.MinimumSize.Width)
							val = this.MinimumSize.Width;
						this.Width = val;
					}
					else
						Settings.Save();
					break;

				case Keys.Alt | Keys.Right:
					if (msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN)
					{
						int val = this.Width + 20;
						var scr = Screen.FromHandle(this.Handle);
						if (this.Width > scr.Bounds.Width - this.Left)
							this.Width = scr.Bounds.Width - this.Left;
						this.Width = val;
					}
					else
						Settings.Save();
					break;

				case Keys.Alt | Keys.Up:
					if (msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN)
					{
						int val = this.Height - 20;
						if (val < this.MinimumSize.Height)
							val = this.MinimumSize.Height;
						this.Height = val;
					}
					else
						Settings.Save();
					break;

				case Keys.Alt | Keys.Down:
					if (msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN)
					{
						int val = this.Height + 20;
						var scr = Screen.FromHandle(this.Handle);
						if (val > scr.Bounds.Height - this.Top)
							val = scr.Bounds.Height - this.Top;
						this.Height = val;
					}
					else
						Settings.Save();
					break;

				//////////////////////////////////////////////////////////////////////////

				case Keys.Control | Keys.S:
					{
						var cur = this.m_viewer.Current;

						if (cur.MediaType == MediaTypes.Image)
						{
							this.sfd.FileName = Path.Combine(this.sfd.InitialDirectory, Path.GetFileName(new Uri(cur.Url.Replace(":orig", "")).AbsolutePath));

							string ext = Path.GetExtension(this.sfd.FileName);
							this.sfd.Filter = string.Format("*{0}|*{0}", ext);

							if (this.sfd.ShowDialog() == DialogResult.OK)
							{
								File.Copy(cur.CachePath, this.sfd.FileName);
								Settings.Save();
							}
						}
					}
					return true;

				case Keys.Control | Keys.R:
					this.m_viewer.DownloadImage();
					return true;
					
				case Keys.G:
				case Keys.Tab:
					NativeMethods.FocusWindow(NativeMethods.FindWindow("Azurea_TwitterClient", null));
					return true;
					
				case Keys.W:
					this.GoBrowser();
					return true;
					
				case Keys.Z:
					this.m_viewer.ToggleZoomMode();
					return true;

				case Keys.V:
					MessageBox.Show(this, Program.TagName, Program.ProgramName, MessageBoxButtons.OK);
					return true;
					
				case Keys.F1:
					Process.Start("http://ryuanerin.github.io/Azpe");
					return true;

				case Keys.Escape:
				case Keys.Enter:
					this.Hide();
					return true;

				case Keys.Shift | Keys.Escape:
					Application.Exit();
					return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void GoBrowser()
		{
			Process.Start(this.m_viewer.Current.OrigUrl);
		}

		private void frmMain_ResizeEnd(object sender, EventArgs e)
		{
			Settings.Save();
		}
	}
}
