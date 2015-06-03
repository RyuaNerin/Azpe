using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Azpe
{
	public partial class FrmInstall : Form
	{
		public FrmInstall()
		{
			InitializeComponent();

			this.Text = string.Format("{0} 설치", Program.ProgramName);
		}

		private void btnInstall_Click(object sender, EventArgs e)
		{
			this.btnInstall.Enabled = false;

			this.Install();

			this.btnInstall.Enabled = true;
		}
		
		private void Install()
		{
			IntPtr	hwnd;
			int		pid;

			// 아즈레아 찾고 path 얻어옴
			if ((hwnd = NativeMethods.FindWindow("Azurea_TwitterClient", null)) == IntPtr.Zero ||
				NativeMethods.GetWindowThreadProcessId(hwnd, out pid) == 0 ||
				pid == 0)
			{
				MessageBox.Show(this, "아즈레아를 실행해주세요!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			
			Process procAz = Process.GetProcessById(pid);

			string pathAz = procAz.MainModule.FileName;

			if (MessageBox.Show(this, "설치를 위해 아즈레아를 종료합니다!", this.Text, MessageBoxButtons.OKCancel) == DialogResult.Cancel)
				return;

			// 아즈레아 종료
			// public const int WM_SYSCOMMAND = 0x0112;
			// public const int SC_CLOSE = 0xF060;
			NativeMethods.SendMessage(hwnd, 0x0112, (IntPtr)0xF060, IntPtr.Zero);
			procAz.WaitForExit();
			
			// azpe 가 이미 동작중인가 체크
			hwnd = NativeMethods.FindWindow(Program.lpClassName, null);
			if (hwnd != IntPtr.Zero)
			{
				NativeMethods.SendData(hwnd, "exit");

				if (NativeMethods.GetWindowThreadProcessId(hwnd, out pid) != 0)
				{
					Process proc = Process.GetProcessById(pid);
					proc.Kill();
					proc.WaitForExit();
				}
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

			MessageBox.Show(this, "설치가 끝났어요!\n아즈레아를 재실행합니다!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

			Process.Start(pathAz);

			this.Close();
		}
	}
}
