using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SciterSharp;
using SciterSharp.Interop;
using PInvoke;

namespace UltraSidebar
{
	class SidebarWindow : SciterWindow
	{
		const   uint WM_APP = 0x8000;
		const	uint WM_CLOSE = 16;
		const	uint WM_NCLBUTTONDOWN = 161;
		
		protected override bool ProcessWindowMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, ref IntPtr lResult)
		{
			if(msg == WM_NCHITTEST)
			{
				if(DwmDefWindowProc(hwnd, msg, wParam, lParam, out lResult) != 0)
					return true;

				PInvokeUtils.POINT p = new PInvokeUtils.POINT() { X = LoWord((int)lParam), Y = HiWord((int)lParam) };
				const int LEFT_WIDTH = 8;
				const int RIGHT_WIDTH = 8;
				const int BOTTOM_WIDTH = 8;
				const int TOP_WIDTH = 8;

				PInvokeUtils.RECT rcWindow;
				GetWindowRect(_hwnd, out rcWindow);

				int uRow = 1;
				int uCol = 1;

				if(p.Y >= rcWindow.top && p.Y < rcWindow.top + TOP_WIDTH)
				{
					uRow = 0;
				}
				else if(p.Y < rcWindow.bottom && p.Y >= rcWindow.bottom - BOTTOM_WIDTH)
				{
					uRow = 2;
				}

				if(p.X >= rcWindow.left && p.X < rcWindow.left + LEFT_WIDTH)
				{
					uCol = 0;
				}
				else if(p.X < rcWindow.right && p.X >= rcWindow.right - RIGHT_WIDTH)
				{
					uCol = 2;
				}

				int[,] hitTests = new int[3, 3] {
						{ HTTOPLEFT,    HTTOP,    HTTOPRIGHT },
						{ HTLEFT,       HTCLIENT,     HTRIGHT },
						{ HTBOTTOMLEFT, HTBOTTOM, HTBOTTOMRIGHT },
					};

				lResult = new IntPtr(hitTests[uRow, uCol]);

				if(hitTests[uRow, uCol] == HTCLIENT)
				{
					ScreenToClient(_hwnd, ref p);
					var el = ElementAtPoint(p.X, p.Y);
					if(el != null && el.Test("caption#area-glass"))
					{
						Debug.WriteLine(el.ToString());
						lResult = new IntPtr(HTCAPTION);
					}
				}

				return true;
			}

			if(_dwm_extended)
			{
				if(msg == WM_NCCALCSIZE)
				{
					bool bCalcValidRects = wParam.ToInt32() != 0;
					if(bCalcValidRects)
					{
						lResult = IntPtr.Zero;
						return true;
					}
				}
			}

			return false;
		}

		

		public void HideTaskbarIcon()
		{
			new Win32Hwnd(Handle).ModifyStyleEx(User32.SetWindowLongFlags.WS_EX_APPWINDOW, User32.SetWindowLongFlags.WS_EX_TOOLWINDOW);
			return;

			const int GWL_EXSTYLE = -20;
			const int WS_EX_TOOLWINDOW = 0x00000080;
			const int WS_EX_LAYERED = 0x00080000;

			SetWindowLong(_hwnd, GWL_EXSTYLE, WS_EX_TOOLWINDOW);
		}
		public void EmulateMoveWnd()
		{
			SendMessageW(_hwnd, WM_NCLBUTTONDOWN, new IntPtr(HTCAPTION), IntPtr.Zero);
		}

		private bool _dwm_extended = false;

		public void ExtendNCArea()
		{
			/*if(Environment.OSVersion.Version.Major >= 10)
			{
				var accent = new AccentPolicy();
				accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

				var accentStructSize = Marshal.SizeOf(accent);

				var accentPtr = Marshal.AllocHGlobal(accentStructSize);
				Marshal.StructureToPtr(accent, accentPtr, false);

				var data = new WindowCompositionAttributeData();
				data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
				data.SizeOfData = accentStructSize;
				data.Data = accentPtr;

				SetWindowCompositionAttribute(_hwnd, ref data);

				Marshal.FreeHGlobal(accentPtr);
			}*/

			bool enabled;
			DwmIsCompositionEnabled(out enabled);
			if(enabled)
			{
				MARGINS margins = new MARGINS();
				margins.leftWidth = -1;
				margins.rightWidth = -1;
				margins.topHeight = -1;
				margins.bottomHeight = -1;
				DwmExtendFrameIntoClientArea(_hwnd, ref margins);

				SciterX.API.SciterSetOption(_hwnd, SciterXDef.SCITER_RT_OPTIONS.SCITER_TRANSPARENT_WINDOW, new IntPtr(1));

				_dwm_extended = true;
			}

			// generate WM_NCCALCSIZE
			SetWindowPos(_hwnd, IntPtr.Zero, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_FRAMECHANGED);
		}

		#region PInvoke stuff
		const uint WM_NCCALCSIZE = 0x0083;
		const uint WM_NCHITTEST = 0x0084;
		const uint WM_WINDOWPOSCHANGED = 0x0047;
		const uint WM_SIZE = 0x0005;

		const int HTERROR = (-2);
		const int HTTRANSPARENT = (-1);
		const int HTNOWHERE = 0;
		const int HTCLIENT = 1;
		const int HTCAPTION = 2;
		const int HTSYSMENU = 3;
		const int HTGROWBOX = 4;
		const int HTSIZE = HTGROWBOX;
		const int HTMENU = 5;
		const int HTHSCROLL = 6;
		const int HTVSCROLL = 7;
		const int HTMINBUTTON = 8;
		const int HTMAXBUTTON = 9;
		const int HTLEFT = 10;
		const int HTRIGHT = 11;
		const int HTTOP = 12;
		const int HTTOPLEFT = 13;
		const int HTTOPRIGHT = 14;
		const int HTBOTTOM = 15;
		const int HTBOTTOMLEFT = 16;
		const int HTBOTTOMRIGHT = 17;
		const int HTBORDER = 18;
		const int HTREDUCE = HTMINBUTTON;
		const int HTZOOM = HTMAXBUTTON;
		const int HTSIZEFIRST = HTLEFT;
		const int HTSIZELAST = HTBOTTOMRIGHT;

		enum WmSizeType
		{
			SIZE_MAXHIDE = 4,
			SIZE_MAXIMIZED = 2,
			SIZE_MAXSHOW = 3,
			SIZE_MINIMIZED = 1,
			SIZE_RESTORED = 0
		}

		static int LoWord(int dwValue)
		{
			return dwValue & 0xFFFF;
		}

		static int HiWord(int dwValue)
		{
			return (dwValue >> 16) & 0xFFFF;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct NCCALCSIZE_PARAMS
		{
			public PInvokeUtils.RECT rect0, rect1, rect2;
			public IntPtr lppos;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct MARGINS
		{
			public int leftWidth;
			public int rightWidth;
			public int topHeight;
			public int bottomHeight;
		}

		enum SetWindowPosWindow : int
		{
			HWND_BOTTOM = 1,
			HWND_NOTOPMOST = -2,
			HWND_TOP = 0,
			HWND_TOPMOST = -1,
		}

		[Flags]
		enum SetWindowPosFlags : uint
		{
			SWP_NOSIZE = 0x0001,
			SWP_NOMOVE = 0x0002,
			SWP_NOZORDER = 0x0004,
			SWP_NOREDRAW = 0x0008,
			SWP_NOACTIVATE = 0x0010,
			SWP_FRAMECHANGED = 0x0020,  /* The frame changed: send WM_NCCALCSIZE */
			SWP_SHOWWINDOW = 0x0040,
			SWP_HIDEWINDOW = 0x0080,
			SWP_NOCOPYBITS = 0x0100,
			SWP_NOOWNERZORDER = 0x0200,  /* Don't do owner Z ordering */
			SWP_NOSENDCHANGING = 0x0400,  /* Don't send WM_WINDOWPOSCHANGING */
			SWP_DRAWFRAME = SWP_FRAMECHANGED,
			SWP_NOREPOSITION = SWP_NOOWNERZORDER,
			SWP_DEFERERASE = 0x2000,
			SWP_ASYNCWINDOWPOS = 0x4000,
		}

		[DllImport("user32.dll")]
		static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

		[DllImport("user32.dll")]
		static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

		[DllImport("dwmapi.dll")]
		static extern int DwmDefWindowProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, out IntPtr result);

		[DllImport("dwmapi.dll")]
		static extern int DwmIsCompositionEnabled(out bool enabled);

		[DllImport("dwmapi.dll")]
		static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

		[DllImport("user32.dll")]
		static extern bool IsZoomed(IntPtr hWnd);

		[DllImport("user32.dll")]
		static extern bool GetWindowRect(IntPtr hwnd, out PInvokeUtils.RECT lpRect);

		[DllImport("user32.dll")]
		static extern bool ScreenToClient(IntPtr hWnd, ref PInvokeUtils.POINT lpPoint);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern uint RegisterWindowMessage(string lpString);

		[DllImport("user32.dll")]
		static extern IntPtr SendMessageW(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		static extern void PostQuitMessage(int nExitCode);

		[DllImport("user32.dll")]
		static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);


		#region Windows 10 SetWindowCompositionAttribute
		internal enum AccentState
		{
			ACCENT_DISABLED = 0,
			ACCENT_ENABLE_GRADIENT = 1,
			ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
			ACCENT_ENABLE_BLURBEHIND = 3,
			ACCENT_INVALID_STATE = 4
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct AccentPolicy
		{
			public AccentState AccentState;
			public int AccentFlags;
			public int GradientColor;
			public int AnimationId;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct WindowCompositionAttributeData
		{
			public WindowCompositionAttribute Attribute;
			public IntPtr Data;
			public int SizeOfData;
		}

		internal enum WindowCompositionAttribute
		{
			// ...
			WCA_ACCENT_POLICY = 19
			// ...
		}

		[DllImport("user32.dll")]
		internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);
		#endregion
		#endregion
	}
}