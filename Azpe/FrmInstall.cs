using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Azpe
{
	public partial class FrmInstall : Form
	{
		public FrmInstall()
		{
			InitializeComponent();

			this.lblVersion.Text = Program.TagName;
		}

		private void btnInstall_Click(object sender, EventArgs e)
		{
			this.btnInstall.Enabled = false;
			this.btnInstall.Text = "설치중입니다";

			this.bgw.RunWorkerAsync();
		}

		private void bgw_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{

			IntPtr	hwnd;
			int		pid;

			// 아즈레아 찾고 path 얻어옴
			if ((hwnd = NativeMethods.FindWindow("Azurea_TwitterClient", null)) == IntPtr.Zero ||
				NativeMethods.GetWindowThreadProcessId(hwnd, out pid) == 0 ||
				pid == 0)
			{
				this.Invoke(new Action(() => MessageBox.Show(this, "아즈레아를 실행해주세요!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)));
				return;
			}

			string pathAz;
			using (var procAz = Process.GetProcessById(pid))
			{
				pathAz = procAz.MainModule.FileName;

				if ((bool)this.Invoke(new Func<bool>(() => MessageBox.Show(this, "설치를 위해 아즈레아를 종료합니다!", this.Text, MessageBoxButtons.OKCancel) == DialogResult.Cancel)))
					return;

				// 아즈레아 종료
				// public const int WM_SYSCOMMAND = 0x0112;
				// public const int SC_CLOSE = 0xF060;
				NativeMethods.SendMessage(hwnd, 0x0112, (IntPtr)0xF060, IntPtr.Zero);
				procAz.WaitForExit();
			}

			// azpe 에 종료 신호를 보낸다
			hwnd = NativeMethods.FindWindow(Program.lpClassName, null);
			if (hwnd != IntPtr.Zero)
			{
				NativeMethods.SendData(hwnd, "exit");

				Thread.Sleep(1000);
			}

			// azpe 찾는다
			var procs = Process.GetProcesses();
			try
			{
				foreach (var proc in procs)
				{
					if (Path.GetFileName(proc.MainModule.FileName) == "azpe.exe" &&
					Path.GetFileName(Path.GetDirectoryName(proc.MainModule.FileName)) == "azpe.js.Private" &&
					Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(proc.MainModule.FileName))) == "scripts")
					{
						proc.Kill();
						proc.WaitForExit();
					}
				}
			}
			catch
			{
			}
			finally
			{
				foreach (var proc in procs)
					proc.Dispose();
				procs = null;
			}

			// 파일 복사			
			string path = Path.Combine(Path.GetDirectoryName(pathAz), "scripts");

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			File.WriteAllText(Path.Combine(path, "azpe.js"), Properties.Resources.script, Encoding.UTF8);

			path = Path.Combine(path, "azpe.js.Private");

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			path = Path.Combine(path, "azpe.exe");
			if (File.Exists(path))
				File.Delete(path);

			File.Copy(Application.ExecutablePath, path);

			// 스크립트 활성화
			NativeMethods.WritePrivateProfileString("Scripting", "EnableScripting", "1", Path.Combine(pathAz, "Azurea.ini8"));

			//////////////////////////////////////////////////////////////////////////

			this.Invoke(new Action(() => MessageBox.Show(this, "설치가 끝났어요!\n아즈레아를 재실행합니다!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information)));

			Process.Start(pathAz).Dispose();

			e.Result = 0;
		}

		private void bgw_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			if (e.Result is int)
				Application.Exit();

			this.btnInstall.Text = "설치";
			this.btnInstall.Enabled = true;
		}
	}
}
