using System;
using System.Runtime.InteropServices;
using System.Security;

namespace AZPreviewE
{
	[SuppressUnmanagedCodeSecurity]
	internal static class WinApi
	{
		public static void FocusWindow(IntPtr hwnd)
		{
			try
			{
				WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
				placement.length = Marshal.SizeOf(placement);
				if (GetWindowPlacement(hwnd, ref placement))
				{
					if (placement.showCmd == ShowWindowCommands.Hide ||
						placement.showCmd == ShowWindowCommands.Minimized)
						WinApi.ShowWindow(hwnd, WinApi.SW_RESTORE);
					SetForegroundWindow(hwnd);
				}
			}
			catch
			{ }
		}
		
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool GetWindowPlacement(
			IntPtr hWnd,
			ref WINDOWPLACEMENT lpwndpl);

		[StructLayout(LayoutKind.Sequential)]
		private struct WINDOWPLACEMENT
		{
			public int length;
			public int flags;
			public ShowWindowCommands showCmd;
			public System.Drawing.Point ptMinPosition;
			public System.Drawing.Point ptMaxPosition;
			public System.Drawing.Rectangle rcNormalPosition;
		}

		private enum ShowWindowCommands : int
		{
			Hide = 0,
			Normal = 1,
			Minimized = 2,
			Maximized = 3,
		}

		//////////////////////////////////////////////////////////////////////////

		[DllImport("user32.dll")]
		public static extern IntPtr SendMessage(
			IntPtr hWnd,
			int Msg,
			IntPtr wParam,
			ref COPYDATASTRUCT lParam);

		[DllImport("user32.dll")]
		public static extern bool SetForegroundWindow(
			IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern IntPtr FindWindow(
			string lpClassName,
			string lpWindowName);


		[StructLayout(LayoutKind.Sequential)]
		public struct COPYDATASTRUCT
		{
			public IntPtr	dwData;
			public int		cbData;
			public IntPtr	lpData;
		}

		public const int WM_COPYDATA = 0x4A;

		[DllImport("user32.dll")]
		public static extern bool ShowWindow(
			IntPtr hWnd,
			int nCmdShow);

		public const int SW_SHOWNORMAL = 1;
		public const int SW_SHOWMAXIMIZED = 3;
		public const int SW_RESTORE = 9;

		//////////////////////////////////////////////////////////////////////////
		
		[DllImport("user32.dll")]
		public static extern IntPtr GetParent(
			IntPtr hwnd);

		//////////////////////////////////////////////////////////////////////////

		public const int ERROR_CLASS_ALREADY_EXISTS = 1410;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WNDCLASS
		{
			public int style;
			public IntPtr lpfnWndProc;
			public int cbClsExtra;
			public int cbWndExtra;
			public IntPtr hInstance;
			public IntPtr hIcon;
			public IntPtr hCursor;
			public IntPtr hbrBackground;
			[MarshalAs(UnmanagedType.LPWStr)]
			public string lpszMenuName;
			[MarshalAs(UnmanagedType.LPWStr)]
			public string lpszClassName;
		}

		[DllImport("user32.dll", SetLastError = true)]
		public static extern ushort RegisterClassW(
			[In] ref WNDCLASS pcWndClassEx);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr CreateWindowExW(
		   int dwExStyle,
		   [MarshalAs(UnmanagedType.LPWStr)]
		   string lpClassName,
		   [MarshalAs(UnmanagedType.LPWStr)]
		   string lpWindowName,
		   int dwStyle,
		   int x,
		   int y,
		   int nWidth,
		   int nHeight,
		   IntPtr hWndParent,
		   IntPtr hMenu,
		   IntPtr hInstance,
		   IntPtr lpParam
		);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr DefWindowProcW(
			IntPtr hWnd,
			int msg,
			IntPtr wParam,
			IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool DestroyWindow(
			IntPtr hWnd);
	}
}
