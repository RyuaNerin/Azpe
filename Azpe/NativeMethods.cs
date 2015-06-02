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
			try
			{
				WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
				placement.length = Marshal.SizeOf(placement);
				if (GetWindowPlacement(hwnd, ref placement))
				{
					if (placement.showCmd == ShowCmds.Hide ||
						placement.showCmd == ShowCmds.Minimized)
						NativeMethods.ShowWindow(hwnd, NativeMethods.SW_RESTORE);
					SetForegroundWindow(hwnd);
				}
			}
			catch
			{ }
		}
		
		public static void SendData(IntPtr hwnd, string msg)
		{
			var buff = System.Text.Encoding.UTF8.GetBytes(msg);

			var st = new NativeMethods.COPYDATASTRUCT();
			st.dwData = Program.wParam;
			st.cbData = buff.Length;
			st.lpData = Marshal.AllocHGlobal(buff.Length);
			Marshal.Copy(buff, 0, st.lpData, buff.Length);

			var lParam = Marshal.AllocHGlobal(Marshal.SizeOf(st));
			Marshal.StructureToPtr(st, lParam, true);

			NativeMethods.SendMessage(hwnd, NativeMethods.WM_COPYDATA, Program.wParam, lParam);

			Marshal.DestroyStructure(lParam, typeof(COPYDATASTRUCT));
			Marshal.FreeHGlobal(lParam);

			Marshal.FreeHGlobal(st.lpData);
		}

		#region Structure
		[StructLayout(LayoutKind.Sequential)]
		private struct WINDOWPLACEMENT
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
		private enum ShowCmds : int
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
		private const int SW_SHOWNORMAL = 1;
		private const int SW_SHOWMAXIMIZED = 3;
		private const int SW_RESTORE = 9;
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
		private static extern bool GetWindowPlacement(
			IntPtr	hWnd,
			ref WINDOWPLACEMENT lpwndpl);

		[DllImport("user32.dll")]
		public static extern IntPtr SendMessage(
			IntPtr	hWnd,
			int		msg,
			IntPtr	wParam,
			IntPtr	lParam);

		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(
			IntPtr	hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr FindWindow(
			string	lpClassName,
			string	lpWindowName);
		
		[DllImport("user32.dll")]
		private static extern bool ShowWindow(
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
