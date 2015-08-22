using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;

using Rect = System.Drawing.Rectangle;

namespace Azpe
{
	[SuppressUnmanagedCodeSecurity]
	internal static class NativeMethods
	{
		public static void FocusWindow(IntPtr hwnd)
		{
		}
		
		public static void SendData(IntPtr hwnd, string msg)
		{
			byte[]	buff = System.Text.Encoding.UTF8.GetBytes(msg);
			IntPtr	lParam = IntPtr.Zero;
			IntPtr	lpData = IntPtr.Zero;

			try
			{
				lpData = Marshal.AllocHGlobal(buff.Length);
				Marshal.Copy(buff, 0, lpData, buff.Length);

				lParam = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeMethods.COPYDATASTRUCT)));
				var data = new COPYDATASTRUCT();
				data.dwData = Program.wParam;
				data.cbData = buff.Length;
				data.lpData = lpData;

				Marshal.StructureToPtr(data, lParam, true);

				NativeMethods.SendMessage(hwnd, WM_COPYDATA, Program.wParam, lParam);
			}
			catch
			{
			}
			finally
			{
				if (lParam != IntPtr.Zero) Marshal.FreeHGlobal(lParam);
				if (lpData != IntPtr.Zero) Marshal.FreeHGlobal(lpData);
			}
		}

		#region Structure
		[StructLayout(LayoutKind.Sequential)]
		public struct WINDOWPLACEMENT
		{
			public int		length;
			public int		flags;
			public ShowCmds showCmd;
			public Point	ptMinPosition;
			public Point	ptMaxPosition;
			public Rect		rcNormalPosition;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct COPYDATASTRUCT
		{
			public IntPtr	dwData;
			public int		cbData;
			public IntPtr	lpData;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WNDCLASS
		{
			public int		style;
			public IntPtr	lpfnWndProc;
			public int		cbClsExtra;
			public int		cbWndExtra;
			public IntPtr	hInstance;
			public IntPtr	hIcon;
			public IntPtr	hCursor;
			public IntPtr	hbrBackground;
			public string	lpszMenuName;
			public string	lpszClassName;
		}
		#endregion

		#region Enum
		public enum ShowCmds : int
		{
			Hide = 0,
			Normal = 1,
			Minimized = 2,
			Maximized = 3,
		}
		#endregion

		#region Constant
		public  const int WM_COPYDATA = 0x4A;
		public  const int ERROR_CLASS_ALREADY_EXISTS = 1410;
		public  const int SW_RESTORE = 9;
		private const int SW_SHOWNORMAL = 1;
		private const int SW_SHOWMAXIMIZED = 3;
		#endregion

		#region Function
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		public static extern int WritePrivateProfileString(
			string	lpApplicationName,
			string	lpKeyName,
			string	lpString,
			string	lpFileName);
		
		[DllImport("user32.dll", SetLastError = true)]
		public static extern int GetWindowThreadProcessId(
			IntPtr	hWnd,
			[Out] out int lpdwProcessId);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetWindowPlacement(
			IntPtr	hWnd,
			ref WINDOWPLACEMENT lpwndpl);

		[DllImport("user32.dll")]
		public static extern IntPtr SendMessage(
			IntPtr	hWnd,
			int		msg,
			IntPtr	wParam,
			IntPtr	lParam);

		[DllImport("user32.dll")]
		public static extern bool SetForegroundWindow(
			IntPtr	hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr FindWindow(
			string	lpClassName,
			string	lpWindowName);
		
		[DllImport("user32.dll")]
		public static extern bool ShowWindow(
			IntPtr	hWnd,
			int		nCmdShow);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern ushort RegisterClass(
			[In] ref WNDCLASS pcWndClassEx);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr CreateWindowEx(
			int		dwExStyle,
			string	lpClassName,
			string	lpWindowName,
			int		dwStyle,
			int		x,
			int		y,
			int		nWidth,
			int		nHeight,
			IntPtr	hWndParent,
			IntPtr	hMenu,
			IntPtr	hInstance,
			IntPtr	lpParam);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr DefWindowProc(
			IntPtr	hWnd,
			int		msg,
			IntPtr	wParam,
			IntPtr	lParam);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool DestroyWindow(
			IntPtr	hWnd);

		#endregion
	}
}
