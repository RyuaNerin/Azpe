using System;
using System.Runtime.InteropServices;

namespace Azpe
{
	class CustomWnd : IDisposable
	{
		public delegate IntPtr CustomProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);
		private delegate IntPtr WndProcD(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		private bool		m_disposed;
		private IntPtr		m_hwnd;
		private WndProcD	m_wndproc;

		private static CustomProc m_custom;

		public void Dispose() 
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) 
		{
			if (!this.m_disposed)
			{
				if (m_hwnd != IntPtr.Zero)
				{
					NativeMethods.DestroyWindow(m_hwnd);
					m_hwnd = IntPtr.Zero;
				}

				this.m_disposed = true;
			}
		}

		public CustomWnd(string className, CustomProc customProc)
		{
			CustomWnd.m_custom = customProc;
			this.m_wndproc = CustomWnd.WndProc;

			var wndClass = new NativeMethods.WNDCLASS();
			wndClass.lpszClassName	= className;
			wndClass.lpfnWndProc	= Marshal.GetFunctionPointerForDelegate(m_wndproc);

			var resRegister	= NativeMethods.RegisterClass(ref wndClass);
			var resError	= Marshal.GetLastWin32Error();

			if (resRegister == 0 && resError != NativeMethods.ERROR_CLASS_ALREADY_EXISTS)
				throw new Exception();

			this.m_hwnd = NativeMethods.CreateWindowEx(0, className, String.Empty, 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

			Console.WriteLine(this.m_hwnd.ToString());
		}

		~CustomWnd()
		{
			this.Dispose(true);
		}

		private static IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
		{
			var handled	= false;
			var result	= CustomWnd.m_custom.Invoke(hWnd, msg, wParam, lParam, ref handled);
			
			return handled ? result : NativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);
		}
	}
}
